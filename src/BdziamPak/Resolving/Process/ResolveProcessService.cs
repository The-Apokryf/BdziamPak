using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Resolving.ResolveSteps.BuiltIn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Resolving;

public class ResolveProcessService(ILogger<ResolveProcessService> logger, IServiceProvider serviceProvider)
    : IResolveProcessService
{
    public async Task ResolveAsync(BdziamPakMetadata bdziamPakMetadata, IProgress<ResolveStatusLog>? progress)
    {
        logger.LogInformation("Resolving BdziamPak {PackageId} version {Version}",
            bdziamPakMetadata.BdziamPakId, bdziamPakMetadata.Version);
        var process = serviceProvider.GetRequiredService<BdziamPakResolveProcess>();
        process.AddStep<ResolveBdziamPakDependenciesStep>()
            .AddStep<CloneRepositoryStep>()
            .AddStep<ResolveNuGetDependenciesStep>();
        process.Progress = progress;

        await process.Execute(bdziamPakMetadata);
        logger.LogInformation("Resolving BdziamPak {PackageId} version {Version} complete",
            bdziamPakMetadata.BdziamPakId, bdziamPakMetadata.Version);
    }
}