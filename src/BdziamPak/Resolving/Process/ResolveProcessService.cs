using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Resolving.ResolveSteps.BuiltIn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Resolving;

/// <summary>
/// Service for resolving BdziamPak packages.
/// </summary>
/// <param name="logger">The logger instance for logging process information.</param>
/// <param name="process">Collection of steps.</param>
public class ResolveProcessService(ILogger<ResolveProcessService> logger, BdziamPakResolveProcess process)
    : IResolveProcessService
{
    /// <summary>
    /// Resolves the specified BdziamPak package metadata.
    /// </summary>
    /// <param name="bdziamPakMetadata">The metadata of the BdziamPak package to resolve.</param>
    /// <param name="progress">The progress reporter for the resolving process.</param>
    /// <returns>A task that represents the asynchronous resolve operation.</returns>
    public async Task ResolveAsync(BdziamPakMetadata bdziamPakMetadata, IProgress<ResolveStatusLog>? progress)
    {
        logger.LogInformation("Resolving BdziamPak {PackageId} version {Version}",
            bdziamPakMetadata.BdziamPakId, bdziamPakMetadata.Version);
        process.AddStep<ResolveBdziamPakDependenciesStep>()
            .AddStep<CloneRepositoryStep>()
            .AddStep<ResolveNuGetDependenciesStep>();
        process.Progress = progress;

        await process.Execute(bdziamPakMetadata);
        logger.LogInformation("Resolving BdziamPak {PackageId} version {Version} complete",
            bdziamPakMetadata.BdziamPakId, bdziamPakMetadata.Version);
    }
}