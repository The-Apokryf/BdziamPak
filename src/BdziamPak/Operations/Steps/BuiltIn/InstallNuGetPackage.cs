using BdziamPak.Directory;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Download;
using BdziamPak.NuGetPackages.Download.Model;
using BdziamPak.NuGetPackages.Unpack;
using BdziamPak.Operations.Context;
using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps.Validation;
using BdziamPak.Operations.Steps.Validation.BuiltIn;
using BdziamPak.PackageModel.Builtin;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace BdziamPak.Operations.Steps.BuiltIn;

/// <summary>
/// Represents a step in the resolving process that resolves NuGet dependencies for the package.
/// </summary>
/// <param name="dependencyResolver">The service used to resolve NuGet dependencies.</param>
/// <param name="nugetDownloadService">The service used to download NuGet packages.</param>
/// <param name="unpackService">The service used to unpack NuGet packages.</param>
/// <param name="bdziamPakDirectory">The directory where the BdziamPak package is located.</param>
public class InstallNuGetPackage(
    NuGetDependencyResolver dependencyResolver,
    NuGetDownloadService nugetDownloadService,
    NuGetUnpackService unpackService,
    BdziamPakDirectory bdziamPakDirectory) : BdziamPakOperationStep
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    public override string StepName => "ResolveNuGetDependencies";

    private const string NuGetMetadataKey = "NuGetPackage";
    public override void ValidateOperation(OperationValidationContext context)
    {
        context.AddCondition<HasMetadataCondition>(condition => condition.RequireMetadata(NuGetMetadataKey));
    }

    public override async Task ExecuteAsync(IExecuteOperationContext context, IProgress<StepProgress> progress, CancellationToken cancellationToken = default)
    {
       progress.Report(("Resolving NuGet Dependencies...", 0, new NuGetDownloadProgress("Starting download", 0)));

        var metadata = context.BdziamPakMetadata;
        var repository = new SourceRepositoryProvider(
            new PackageSourceProvider(Settings.LoadDefaultSettings(null)),
            Repository.Provider.GetCoreV3()
        ).GetRepositories().First();
        var nugetDependency = context.GetMetadata<BdziamPakNuGetDependency>("NuGetPackage")!;
        var packages = await dependencyResolver.LoadPackageDependenciesAsync(
            nugetDependency.PackageId,
            NuGetVersion.Parse(nugetDependency.PackageVersion),
            repository
        );

        foreach (var package in packages)
        {
            var nugetProgress = new Progress<NuGetDownloadProgress>();
            await nugetDownloadService.DownloadPackageAsync(
                package.Id,
                package.Version.ToString(),
                bdziamPakDirectory.CacheDirectory.FullName,
                nugetProgress
            );

            var currentPercent = 0;
            
            nugetProgress.ProgressChanged += (sender, downloadProgress) =>
            {
                currentPercent = downloadProgress.Percent ?? 0;
                progress.Report(("Downloading NuGet Package...", currentPercent, downloadProgress));
            };

            var unpackProgress = new Progress<string>();
            unpackProgress.ProgressChanged += (sender, s) =>
            {
                progress.Report(($"Unpacking ({s})", currentPercent, s));
            };
            var unpackPath = Path.Combine(context.ResolveDirectory.FullName, "Lib");
            await unpackService.UnpackPackageAsync(unpackPath, package, unpackProgress, cancellationToken);
        }

        StepState = StepState.Success;
    }

}