﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using BdziamPak.Directory;
using BdziamPak.PackageModel;
using BdziamPak.Sources.Model;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Sources;

/// <summary>
///     Manages the sources for BdziamPak packages, including caching, loading, and searching.
/// </summary>
public class Sources : IDisposable
{
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger _logger;

    // Cache structure with last update time
    private readonly ConcurrentDictionary<string, (BdziamPakSourceIndex Source, DateTime LastUpdate)> _sourceCache;
    private readonly DirectoryInfo _sourcesDirectory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Sources" /> class.
    /// </summary>
    /// <param name="bdziamPakDirectory">The directory containing the sources.</param>
    /// <param name="logger">The logger instance.</param>
    public Sources(BdziamPakDirectory bdziamPakDirectory, ILogger<Sources> logger)
    {
        _sourcesDirectory = bdziamPakDirectory.SourcesDirectory;
        _logger = logger;
        _httpClient = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _sourceCache = new ConcurrentDictionary<string, (BdziamPakSourceIndex, DateTime)>();

        _logger.LogDebug("Sources initialized with directory: {Directory}", _sourcesDirectory.FullName);
    }

    /// <summary>
    ///     Disposes the resources used by the <see cref="Sources" /> instance.
    /// </summary>
    public void Dispose()
    {
        _logger.LogDebug("Disposing Sources instance");
        _httpClient.Dispose();
        _cacheLock.Dispose();
    }

    /// <summary>
    ///     Loads a source file asynchronously.
    /// </summary>
    /// <param name="file">The file to load.</param>
    /// <returns>The loaded source index, or null if an error occurs.</returns>
    private async Task<BdziamPakSourceIndex?> LoadSourceFileAsync(FileInfo file, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading source file: {FilePath}", file.FullName);
        try
        {
            await using var stream = file.OpenRead();
            var source =
                await JsonSerializer.DeserializeAsync<BdziamPakSourceIndex>(stream, _jsonOptions, cancellationToken);
            _logger.LogDebug("Successfully loaded source file: {FilePath}, Source Name: {SourceName}",
                file.FullName, source?.Name ?? "null");
            return source;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing source file {FilePath}", file.FullName);
            return null;
        }
    }

    /// <summary>
    ///     Refreshes the cache if needed.
    /// </summary>
    private async Task RefreshCacheIfNeededAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Attempting to acquire cache refresh lock");
        if (!await _cacheLock.WaitAsync(TimeSpan.FromSeconds(10)))
        {
            _logger.LogWarning("Cache refresh lock timeout");
            return;
        }

        try
        {
            var now = DateTime.UtcNow;
            var staleSources = _sourceCache
                .Where(kvp => now - kvp.Value.LastUpdate > _cacheExpiration)
                .Select(kvp => kvp.Key)
                .ToList();

            _logger.LogDebug("Found {Count} stale sources in cache", staleSources.Count);

            if (!staleSources.Any())
            {
                _logger.LogDebug("No stale sources to refresh");
                return;
            }

            _logger.LogInformation("Refreshing cache for {Count} stale sources: {Sources}",
                staleSources.Count, string.Join(", ", staleSources));

            if (!_sourcesDirectory.Exists)
            {
                _logger.LogWarning("Sources directory does not exist, clearing cache");
                _sourceCache.Clear();
                return;
            }

            var tasks = staleSources.Select(async sourceName =>
            {
                var filePath = Path.Combine(_sourcesDirectory.FullName, $"{sourceName}.json");
                var file = new FileInfo(filePath);
                if (!file.Exists)
                {
                    _logger.LogInformation("Removing non-existent source from cache: {SourceName}",
                        sourceName);
                    _sourceCache.TryRemove(sourceName, out _);
                    return;
                }

                var source = await LoadSourceFileAsync(file, cancellationToken);
                if (source != null)
                {
                    _logger.LogDebug("Updated cache for source: {SourceName}", sourceName);
                    _sourceCache[sourceName] = (source, now);
                }
            });

            await Task.WhenAll(tasks);
        }
        finally
        {
            _cacheLock.Release();
            _logger.LogDebug("Released cache refresh lock");
        }
    }

    /// <summary>
    ///     Lists the sources asynchronously.
    /// </summary>
    /// <returns>A list of source indexes.</returns>
    public async Task<IReadOnlyList<BdziamPakSourceIndex>> ListSourcesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Listing sources, initiating cache refresh");
        await RefreshCacheIfNeededAsync();

        if (!_sourcesDirectory.Exists)
        {
            _logger.LogWarning("Sources directory does not exist");
            return Array.Empty<BdziamPakSourceIndex>();
        }

        var sourceFiles = _sourcesDirectory.GetFiles("*.json");
        _logger.LogDebug("Found {Count} source files in directory", sourceFiles.Length);

        var now = DateTime.UtcNow;
        var tasks = sourceFiles.Select(async file =>
        {
            var sourceName = Path.GetFileNameWithoutExtension(file.Name);
            _logger.LogDebug("Processing source file: {SourceName}", sourceName);

            if (_sourceCache.TryGetValue(sourceName, out var cached) &&
                now - cached.LastUpdate <= _cacheExpiration)
            {
                _logger.LogDebug("Using cached version of source: {SourceName}", sourceName);
                return cached.Source;
            }

            var source = await LoadSourceFileAsync(file, cancellationToken);
            if (source != null)
            {
                _logger.LogDebug("Caching newly loaded source: {SourceName}", sourceName);
                _sourceCache[sourceName] = (source, now);
            }

            return source;
        });

        var sources = await Task.WhenAll(tasks);
        var validSources = sources.Where(s => s != null).ToImmutableList()!;
        _logger.LogDebug("Returning {Count} valid sources", validSources.Count);
        return validSources;
    }

    /// <summary>
    ///     Registers a new source from a URL asynchronously.
    /// </summary>
    /// <param name="url">The URL of the source to register.</param>
    public async Task RegisterSourceAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering new source from URL: {Url}", url);
        try
        {
            var sourceJson = await _httpClient.GetStringAsync(url);
            _logger.LogDebug("Successfully downloaded source JSON from: {Url}", url);

            var source = JsonSerializer.Deserialize<BdziamPakSourceIndex>(sourceJson, _jsonOptions);

            if (source?.Name == null)
            {
                _logger.LogError("Invalid source: Name is required. URL: {Url}", url);
                throw new InvalidOperationException("Invalid source: Name is required");
            }

            _sourcesDirectory.Create();
            var filePath = Path.Combine(_sourcesDirectory.FullName, $"{source.Name}.json");

            await File.WriteAllTextAsync(filePath, sourceJson, cancellationToken);
            _sourceCache[source.Name] = (source, DateTime.Now);

            _logger.LogInformation("Successfully registered source: {SourceName}", source.Name);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to register source from URL: {Url}", url);
            throw;
        }
    }

    /// <summary>
    ///     Deletes a source by name.
    /// </summary>
    /// <param name="name">The name of the source to delete.</param>
    public void DeleteSource(string name)
    {
        _logger.LogInformation("Attempting to delete source: {SourceName}", name);
        var filePath = Path.Combine(_sourcesDirectory.FullName, $"{name}.json");
        if (File.Exists(filePath))
            try
            {
                File.Delete(filePath);
                _sourceCache.TryRemove(name, out _);
                _logger.LogInformation("Successfully deleted source: {SourceName}", name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete source: {SourceName}", name);
                throw;
            }
        else
            _logger.LogWarning("Source file not found for deletion: {SourceName}", name);
    }

    /// <summary>
    ///     Searches for packages matching the search term asynchronously.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <param name="comparison">The string comparison method to use.</param>
    /// <returns>A list of matching packages and their source names.</returns>
    public async Task<IReadOnlyList<(BdziamPakMetadata Package, string SourceName)?>> SearchAsync(
        string searchTerm,
        StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        _logger.LogDebug("Starting search for term: {SearchTerm}", searchTerm);
        await RefreshCacheIfNeededAsync();

        var results = new ConcurrentBag<(BdziamPakMetadata, string)?>();
        var sources = await ListSourcesAsync();

        _logger.LogDebug("Searching across {Count} sources", sources.Count);

        await Parallel.ForEachAsync(sources, async (source, ct) =>
        {
            var matches = source.Paks
                .Where(pak =>
                    pak.BdziamPakId.Contains(searchTerm, comparison) ||
                    (pak.Description?.Contains(searchTerm, comparison) ?? false))
                .ToList();

            if (matches.Any())
            {
                _logger.LogDebug("Found {Count} matches in source {SourceName}",
                    matches.Count, source.Name);

                foreach (var match in matches) results.Add((match, source.Name));
            }
        });

        var finalResults = results.ToImmutableList();
        _logger.LogInformation("Search completed. Found {Count} total matches for term: {SearchTerm}",
            finalResults.Count, searchTerm);

        return finalResults;
    }
}