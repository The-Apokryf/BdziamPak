﻿using System.IO;
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

    private string GetPackagePath(string packageId, string version)
    {
        return Path.Combine(directory.CacheDirectory.FullName, $"{packageId}.{version}.nupkg");
    }
    
    public string CacheDirectoryPath => directory.CacheDirectory.FullName;
}