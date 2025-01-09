using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Packaging.Install.Model;
using BdziamPak.Resolving;
using Microsoft.Extensions.Logging;

public class BdziamPakService
{
    private readonly ILogger<BdziamPakService> _logger;
    private readonly IResolveProcessService _resolveProcessService;
    private readonly Sources _sources;

    public BdziamPakService(
        ILogger<BdziamPakService> logger,
        IResolveProcessService resolveProcessService,
        Sources sources)
    {
        _logger = logger;
        _resolveProcessService = resolveProcessService;
        _sources = sources;
    }

    public async Task<BdziamPakInstallResult> ResolveBdziamPakAsync(
        string bdziamPakId,
        string version,
        IProgress<BdziamPakInstallProgress> progress)
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

            var resolveProgress = new Progress<ResolveStatusLog>(log =>
            {
                progress.Report(new BdziamPakInstallProgress
                {
                    Message = log.Message
                });
            });

            await _resolveProcessService.ResolveAsync(metadata, resolveProgress);

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

    private async Task<BdziamPakMetadata?> LoadMetadataAsync(string bdziamPakId, string version)
    {
        var searchResults = await _sources.SearchAsync(bdziamPakId);
        return searchResults
            .FirstOrDefault(r => r?.Package.BdziamPakId == bdziamPakId && r?.Package.Version == version)
            ?.Package;
    }
}