using BdziamPak.Structure;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Packages.NuGet;

/// <summary>
/// Provides functionality for caching NuGet packages.
/// </summary>
public class NuGetCache
{
    private readonly BdziamPakDirectory directory;
    private readonly ILogger<NuGetCache> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetCache"/> class.
    /// </summary>
    /// <param name="directory">The directory where the cache is stored.</param>
    /// <param name="logger">The logger instance for logging.</param>
    public NuGetCache(BdziamPakDirectory directory, ILogger<NuGetCache> logger)
    {
        this.directory = directory;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the path to the cache directory.
    /// </summary>
    public string CacheDirectoryPath => directory.CacheDirectory.FullName;

    /// <summary>
    /// Checks if the specified package is cached.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <returns>True if the package is cached, otherwise false.</returns>
    public bool IsPackageCached(string packageId, string version)
    {
        var packagePath = GetPackagePath(packageId, version);
        logger.LogDebug("Checking if package {PackageId} version {Version} is cached at {PackagePath}", packageId, version, packagePath);
        return File.Exists(packagePath);
    }

    /// <summary>
    /// Gets the path to the specified package in the cache.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <returns>The path to the package in the cache.</returns>
    public string GetPackagePath(string packageId, string version)
    {
        return Path.Combine(directory.CacheDirectory.FullName, $"{packageId}.{version}.nupkg");
    }
}