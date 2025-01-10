using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Packaging.Install.Model;
using BdziamPak.Resolving;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for resolving BdziamPak packages.
/// </summary>
public class BdziamPakService
{
    private readonly ILogger<BdziamPakService> _logger;
    private readonly IResolveProcessService _resolveProcessService;
    private readonly Sources _sources;

    /// <summary>
    /// Initializes a new instance of the <see cref="BdziamPakService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="resolveProcessService">The resolve process service.</param>
    /// <param name="sources">The sources for searching packages.</param>
    public BdziamPakService(
        ILogger<BdziamPakService> logger,
        IResolveProcessService resolveProcessService,
        Sources sources)
    {
        _logger = logger;
        _resolveProcessService = resolveProcessService;
        _sources = sources;
    }

    /// <summary>
    /// Resolves a BdziamPak package asynchronously.
    /// </summary>
    /// <param name="bdziamPakId">The ID of the BdziamPak package.</param>
    /// <param name="version">The version of the BdziamPak package.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <returns>The result of the BdziamPak package installation.</returns>
    public async Task<BdziamPakInstallResult> ResolveBdziamPakAsync(
        string bdziamPakId,
        string version,
        IProgress<BdziamPakResolveProgress> progress)
    {
        try
        {
            var metadata = await LoadMetadataAsync(bdziamPakId, version);
            if (metadata == null)
                return new BdziamPakInstallResult
                {
                    Success = false,
                    Message = $"Package {bdziamPakId} v{version} not found"
                };

            var resolveProgress = new BdziamPakResolveProgress();
            var resolveStatusProgress = new Progress<ResolveStatusLog>(log =>
            {
                resolveProgress.ResolveStatusLogs.Add(log);
                resolveProgress.Message = log.Message;
                progress.Report(resolveProgress);
            });

            await _resolveProcessService.ResolveAsync(metadata, resolveStatusProgress);

            return new BdziamPakInstallResult
            {
                Success = true,
                Message = $"Successfully resolved {bdziamPakId} v{version}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve BdziamPak {BdziamPakId} v{Version}", bdziamPakId, version);
            throw;
        }
    }

    /// <summary>
    /// Loads the metadata for a BdziamPak package asynchronously.
    /// </summary>
    /// <param name="bdziamPakId">The ID of the BdziamPak package.</param>
    /// <param name="version">The version of the BdziamPak package.</param>
    /// <returns>The metadata of the BdziamPak package if found; otherwise, null.</returns>
    private async Task<BdziamPakMetadata?> LoadMetadataAsync(string bdziamPakId, string version)
    {
        var searchResults = await _sources.SearchAsync(bdziamPakId);
        return searchResults
            .FirstOrDefault(r => r?.Package.BdziamPakId == bdziamPakId && r?.Package.Version == version)
            ?.Package;
    }
}