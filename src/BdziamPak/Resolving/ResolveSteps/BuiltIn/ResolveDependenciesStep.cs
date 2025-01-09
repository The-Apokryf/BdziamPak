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

public class ResolveNuGetDependenciesStep(
    NuGetDependencyResolver dependencyResolver,
    NuGetDownloadService nugetDownloadService,
    NuGetUnpackService unpackService,
    BdziamPakDirectory bdziamPakDirectory) : BdziamPakResolveStep
{
    public override string StepName => "ResolveNuGetDependencies";
    public override string StepDescription => "Resolves NuGet dependencies for the package.";

    public override bool CanExecute(ICheckResolveContext context)
    {
        return context.HasMetadata("NuGetPackage");
    }

    public override async Task ExecuteAsync(IExecutionResolveContext context)
    {
        context.UpdateStatus("Resolving NuGet dependencies...");

        var metadata = context.BdziamPakMetadata;
        var repository = new SourceRepositoryProvider(
            new PackageSourceProvider(Settings.LoadDefaultSettings(null)),
            Repository.Provider.GetCoreV3()
        ).GetRepositories().First();
        var nugetDependency = metadata.GetMetadata<BdziamPakNuGetDependency>("NuGetPackage")!;
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