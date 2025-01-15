using System.Text.Json;
using BdziamPak.PackageModel;
using BdziamPak.Sources.Model;
using Directory = System.IO.Directory;
namespace BdziamPak.PakRepoApi.Services;

/// <summary>
/// Service for managing BdziamPak index operations.
/// </summary>
public class BdziamPakIndexService
{
    private const string IndexJson = "index.json";
    private readonly string _indexFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly BdziamPakSourceIndex _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="BdziamPakIndexService"/> class.
    /// </summary>
    /// <param name="dataDirectory">The directory where the index file is stored.</param>
    public BdziamPakIndexService(string dataDirectory)
    {
        _indexFilePath = Path.Combine(dataDirectory, IndexJson);

        if (!System.IO.Directory.Exists(dataDirectory)) System.IO.Directory.CreateDirectory(dataDirectory);

        if (!File.Exists(_indexFilePath))
        {
            _source = new BdziamPakSourceIndex { Paks = new List<BdziamPakMetadata>() };
            File.WriteAllText(_indexFilePath, JsonSerializer.Serialize(_source));
        }
        else
        {
            var json = File.ReadAllText(_indexFilePath);
            _source = JsonSerializer.Deserialize<BdziamPakSourceIndex>(json) ?? new BdziamPakSourceIndex
                { Paks = new List<BdziamPakMetadata>() };
        }
    }

    /// <summary>
    /// Registers the given metadata.
    /// </summary>
    /// <param name="metadata">The metadata to register.</param>
    public async Task RegisterMetadataAsync(BdziamPakMetadata metadata)
    {
        await _semaphore.WaitAsync();
        try
        {
            _source.Paks.Add(metadata);
            await SaveIndexAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Removes the metadata with the specified ID and version.
    /// </summary>
    /// <param name="bdziamPakId">The ID of the metadata to remove.</param>
    /// <param name="version">The version of the metadata to remove.</param>
    public async Task RemoveVersionAsync(string bdziamPakId, string version)
    {
        await _semaphore.WaitAsync();
        try
        {
            var metadata = _source.Paks.FirstOrDefault(p => p.BdziamPakId == bdziamPakId && p.VersionExists(version));
            if (metadata != null)
            {
                var foundVersion = metadata.Versions.FirstOrDefault(x => x.Version == version);
                if (foundVersion != null)
                {
                    metadata.Versions.Remove(foundVersion);
                    await SaveIndexAsync();
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Edits the source with the new name and description.
    /// </summary>
    /// <param name="newName">The new name for the source.</param>
    /// <param name="newDescription">The new description for the source.</param>
    public async Task EditSourceAsync(string newName, string newDescription)
    {
        await _semaphore.WaitAsync();
        try
        {
            _source.Name = newName;
            _source.Description = newDescription;
            await SaveIndexAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Saves the index to the file.
    /// </summary>
    private async Task SaveIndexAsync()
    {
        var json = JsonSerializer.Serialize(_source);
        await File.WriteAllTextAsync(_indexFilePath, json);
    }

    /// <summary>
    /// Gets the path to the index file.
    /// </summary>
    /// <returns>The path to the index file.</returns>
    public string GetIndexFilePath()
    {
        return _indexFilePath;
    }
}