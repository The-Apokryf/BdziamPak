using BdziamPak.NuGetPackages;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Model;
using BdziamPak.NuGetPackages.Unpack;
using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Structure;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace BdziamPak.Resolving.ResolveSteps.BuiltIn;

/// <summary>
/// Represents a step in the resolving process that resolves NuGet dependencies for the package.
/// </summary>
/// <param name="dependencyResolver">The service used to resolve NuGet dependencies.</param>
/// <param name="nugetDownloadService">The service used to download NuGet packages.</param>
/// <param name="unpackService">The service used to unpack NuGet packages.</param>
/// <param name="bdziamPakDirectory">The directory where the BdziamPak package is located.</param>
public class ProcessNuGetDependenciesStep(
    NuGetDependencyResolver dependencyResolver,
    NuGetDownloadService nugetDownloadService,
    NuGetUnpackService unpackService,
    BdziamPakDirectory bdziamPakDirectory) : BdziamPakProcessStep
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    public override string StepName => "ResolveNuGetDependencies";

    /// <summary>
    /// Determines whether the step can be executed based on the provided context.
    /// </summary>
    /// <param name="context">The context to check for execution eligibility.</param>
    /// <returns><c>true</c> if the step can be executed; otherwise, <c>false</c>.</returns>
    public override bool CanExecute(ICheckProcessingContext context)
    {
        return context.HasMetadata("NuGetPackage");
    }

    /// <summary>
    /// Executes the step asynchronously.
    /// </summary>
    /// <param name="context">The context for the execution of the step.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExecuteAsync(IExecutionProcessingContext context)
    {
        context.UpdateStatus("Resolving NuGet dependencies...");

        var metadata = context.BdziamPakMetadata;
        var repository = new SourceRepositoryProvider(
            new PackageSourceProvider(Settings.LoadDefaultSettings(null)),
            Repository.Provider.GetCoreV3()
        ).GetRepositories().First();
        var nugetDependency = metadata.BdziamPakVersion.GetMetadata<BdziamPakNuGetDependency>("NuGetPackage")!;
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

            var unpackPath = Path.Combine(context.ResolveDirectory.FullName, "Lib");
            await unpackService.UnpackPackageAsync(unpackPath, package, CancellationToken.None);
        }

        context.Complete();
    }
}