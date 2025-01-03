using System.IO;
using BdziamPak.Structure;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Packages.NuGet;

public class NuGetCache(BdziamPakDirectory directory, ILogger<NuGetCache> logger)
{
    public bool IsPackageCached(string packageId, string version)
    {
        var packagePath = GetPackagePath(packageId, version);
        logger.LogDebug("Checking if package {PackageId} version {Version} is cached at {PackagePath}", packageId, version, packagePath);
        return File.Exists(packagePath);
    }

    public void AddPackageToCache(string packageId, string version, string packageFilePath)
    {
        var targetPath = GetPackagePath(packageId, version);
        logger.LogDebug("Adding package {PackageId} version {Version} to cache at {TargetPath}", packageId, version, targetPath);

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.Copy(packageFilePath, targetPath, true);

        logger.LogInformation("Package {PackageId} version {Version} added to cache at {TargetPath}", packageId, version, targetPath);
    }

    private string GetPackagePath(string packageId, string version)
    {
        return Path.Combine(directory.CacheDirectory.FullName, packageId, version, $"{packageId}.{version}.nupkg");
    }
}