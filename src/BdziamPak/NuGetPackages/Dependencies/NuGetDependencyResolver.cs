using BdziamPak.NuGetPackages.Logging;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ILogger = NuGet.Common.ILogger;

namespace BdziamPak.NuGetPackages.Dependencies;

/// <summary>
/// Provides functionality for resolving NuGet package dependencies.
/// </summary>
/// <param name="logger">Injected logger</param>
public class NuGetDependencyResolver(ILogger<NuGetDependencyResolver> logger)
{
    private readonly ILogger _logger = new NuGetLoggerWrapper(logger);

    /// <summary>
    /// Resolves the dependencies for a given NuGet package.
    /// </summary>
    /// <param name="packageId">The ID of the NuGet package.</param>
    /// <param name="version">The version of the NuGet package.</param>
    /// <param name="sourceRepository">The source repository to resolve dependencies from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of resolved package dependencies.</returns>
    public async Task<IEnumerable<SourcePackageDependencyInfo>> LoadPackageDependenciesAsync(
        string packageId,
        NuGetVersion version,
        SourceRepository sourceRepository)
    {
        logger.LogDebug("Loading dependencies for package {PackageId} {Version}", packageId, version);

        var framework = await GetBestFrameworkAsync(packageId, version, sourceRepository);
        logger.LogInformation("Using framework {Framework} for dependency resolution", framework);

        return await LoadPackageDependenciesAsync(packageId, version, sourceRepository, framework);
    }

    /// <summary>
    /// Resolves the dependencies for a given NuGet package with a specified target framework.
    /// </summary>
    /// <param name="packageId">The ID of the NuGet package.</param>
    /// <param name="version">The version of the NuGet package.</param>
    /// <param name="sourceRepository">The source repository to resolve dependencies from.</param>
    /// <param name="targetFramework">The target framework for dependency resolution.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of resolved package dependencies.</returns>
    public async Task<IEnumerable<SourcePackageDependencyInfo>> LoadPackageDependenciesAsync(
        string packageId,
        NuGetVersion version,
        SourceRepository sourceRepository,
        NuGetFramework targetFramework)
    {
        try
        {
            if (IsRuntimePackage(new SourcePackageDependencyInfo(packageId, version, Array.Empty<PackageDependency>(), true, null)))
            {
                logger.LogInformation("Package {PackageId} is a runtime package, skipping dependency resolution", packageId);
                return [];
            }

            logger.LogDebug("Creating source cache context for dependency resolution");
            using var sourceCacheContext = new SourceCacheContext();

            logger.LogTrace("Getting dependency info resource from repository");
            var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();

            logger.LogDebug("Resolving package {PackageId} {Version} dependencies", packageId, version);
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                new PackageIdentity(packageId, version),
                targetFramework,
                sourceCacheContext,
                _logger,
                CancellationToken.None);

            if (dependencyInfo == null)
            {
                logger.LogWarning("Package {PackageId} {Version} not found", packageId, version);
                throw new InvalidOperationException($"Package {packageId} {version} not found");
            }

            logger.LogTrace("Creating collection for available packages");
            var availablePackages = new HashSet<SourcePackageDependencyInfo>();

            logger.LogDebug("Starting recursive dependency resolution for {PackageId} {Version}", packageId, version);
            await GetAllDependenciesRecursiveAsync(
                dependencyInfo,
                targetFramework,
                availablePackages,
                dependencyInfoResource,
                sourceCacheContext);

            var nonRuntimePackages = availablePackages
                .Where(p => !IsRuntimePackage(p))
                .ToList();

            logger.LogInformation(
                "Successfully resolved {DependencyCount} dependencies for package {PackageId} {Version} (excluded {ExcludedCount} runtime packages)",
                nonRuntimePackages.Count,
                packageId,
                version,
                availablePackages.Count - nonRuntimePackages.Count);

            return nonRuntimePackages;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving dependencies for package {PackageId} {Version}", packageId, version);
            throw;
        }
    }

    /// <summary>
    /// Recursively resolves all dependencies for a given package.
    /// </summary>
    /// <param name="package">The package to resolve dependencies for.</param>
    /// <param name="targetFramework">The target framework for dependency resolution.</param>
    /// <param name="availablePackages">A collection of already resolved packages.</param>
    /// <param name="dependencyInfoResource">The dependency info resource.</param>
    /// <param name="sourceCacheContext">The source cache context.</param>
    private async Task GetAllDependenciesRecursiveAsync(
        SourcePackageDependencyInfo package,
        NuGetFramework targetFramework,
        HashSet<SourcePackageDependencyInfo> availablePackages,
        DependencyInfoResource dependencyInfoResource,
        SourceCacheContext sourceCacheContext)
    {
        if (IsRuntimePackage(package))
        {
            logger.LogDebug("Skipping runtime package {PackageId} {Version}", package.Id, package.Version);
            return;
        }

        if (availablePackages.Add(package))
        {
            logger.LogTrace("Added package {PackageId} {Version} to dependency tree", package.Id, package.Version);

            foreach (var dependency in package.Dependencies.Where(d => !IsRuntimePackage(
                         new SourcePackageDependencyInfo(
                             d.Id, d.VersionRange.MinVersion ?? NuGetVersion.Parse("0.0.0"),
                             Array.Empty<PackageDependency>(), true, null))))
            {
                logger.LogTrace("Resolving dependency {DependencyId} {VersionRange} for package {PackageId}", dependency.Id, dependency.VersionRange, package.Id);

                var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                    new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                    targetFramework,
                    sourceCacheContext,
                    _logger,
                    CancellationToken.None);

                if (dependencyInfo != null)
                {
                    logger.LogDebug("Found dependency {DependencyId} {Version}", dependencyInfo.Id, dependencyInfo.Version);

                    await GetAllDependenciesRecursiveAsync(
                        dependencyInfo,
                        targetFramework,
                        availablePackages,
                        dependencyInfoResource,
                        sourceCacheContext);
                }
                else
                {
                    logger.LogWarning("Dependency {DependencyId} {VersionRange} not found", dependency.Id, dependency.VersionRange);
                }
            }
        }
        else
        {
            logger.LogTrace("Package {PackageId} {Version} already processed, skipping", package.Id, package.Version);
        }
    }

    /// <summary>
    /// Determines the best framework for a given package.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <param name="sourceRepository">The source repository to get the package from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the best framework for the package.</returns>
    public async Task<NuGetFramework> GetBestFrameworkAsync(
        string packageId,
        NuGetVersion version,
        SourceRepository sourceRepository)
    {
        try
        {
            logger.LogDebug("Determining best framework for package {PackageId} {Version}", packageId, version);

            var resource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>();
            using var sourceCacheContext = new SourceCacheContext();

            logger.LogTrace("Getting package information from repository");
            using var packageStream = new MemoryStream();
            var packageExists = await resource.CopyNupkgToStreamAsync(
                packageId,
                version,
                packageStream,
                sourceCacheContext,
                _logger,
                CancellationToken.None);

            if (!packageExists)
            {
                logger.LogWarning("Package {PackageId} {Version} not found", packageId, version);
                throw new InvalidOperationException($"Package {packageId} {version} not found");
            }

            packageStream.Position = 0;
            using var packageReader = new PackageArchiveReader(packageStream);
            var frameworks = packageReader.GetSupportedFrameworks().ToList();

            logger.LogDebug("Found {Count} supported frameworks for package {PackageId}: {Frameworks}", frameworks.Count, packageId, string.Join(", ", frameworks.Select(f => f.ToString())));

            // First try to find .NET Standard
            var netStandard = frameworks
                .Where(f => f.Framework.Equals(".NETStandard"))
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();

            if (netStandard != null)
            {
                logger.LogInformation("Selected .NET Standard {Version} framework", netStandard.Version);
                return netStandard;
            }

            // Then try .NET Core
            var netCore = frameworks
                .Where(f => f.Framework.Equals(".NETCoreApp"))
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();

            if (netCore != null)
            {
                logger.LogInformation("Selected .NET Core {Version} framework", netCore.Version);
                return netCore;
            }

            // Finally try .NET Framework
            var netFramework = frameworks
                .Where(f => f.Framework.Equals(".NETFramework"))
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();

            if (netFramework != null)
            {
                logger.LogInformation("Selected .NET Framework {Version} framework", netFramework.Version);
                return netFramework;
            }

            // If nothing matches, fall back to the newest framework version
            logger.LogWarning("No specific framework match found, falling back to newest supported framework");
            var fallback = frameworks.OrderByDescending(f => f.Version).FirstOrDefault()
                           ?? NuGetFramework.Parse("net8.0");

            logger.LogInformation("Selected fallback framework: {Framework}", fallback);
            return fallback;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error determining framework for package {PackageId} {Version}", packageId, version);
            throw;
        }
    }

    /// <summary>
    /// Determines if a given package is a runtime package.
    /// </summary>
    /// <param name="package">The package to check.</param>
    /// <returns>True if the package is a runtime package, otherwise false.</returns>
    private bool IsRuntimePackage(SourcePackageDependencyInfo package)
    {
        // System packages that are part of the framework
        if (package.Id.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
            package.Id.StartsWith("Microsoft.NETCore.", StringComparison.OrdinalIgnoreCase) ||
            package.Id.StartsWith("runtime.", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("Package {PackageId} identified as runtime package (prefix match)", package.Id);
            return true;
        }

        // Special framework packages
        switch (package.Id.ToLowerInvariant())
        {
            case "microsoft.csharp":
            case "microsoft.netcore.app":
            case "microsoft.aspnetcore.app":
            case "microsoft.windowsdesktop.app":
            case "netstandard.library":
            case "microsoft.netcore.platforms":
                logger.LogDebug("Package {PackageId} identified as runtime package (known framework package)", package.Id);
                return true;
            default:
                return false;
        }
    }
}