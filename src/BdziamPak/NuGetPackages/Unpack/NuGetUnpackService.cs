using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Logging;
using BdziamPak.Packages.NuGet;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using ILogger = NuGet.Common.ILogger;

namespace BdziamPak.NuGetPackages.Unpack;

public class NuGetUnpackService(
    ILogger<NuGetUnpackService> logger,
    NuGetCache nuGetCache,
    NuGetDependencyResolver dependencyResolver)
{
    private readonly ILogger _logger = new NuGetLoggerWrapper(logger);

    public async Task<string> UnpackPackageAsync(
        string extractPath,
        SourcePackageDependencyInfo packageInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Starting package extraction for {PackageId} {Version} to {Path}",
                packageInfo.Id, packageInfo.Version, extractPath);

            if (string.IsNullOrEmpty(extractPath))
            {
                logger.LogError("Extract path cannot be null or empty");
                throw new ArgumentNullException(nameof(extractPath));
            }

            if (packageInfo == null)
            {
                logger.LogError("Package information cannot be null");
                throw new ArgumentNullException(nameof(packageInfo));
            }

            var packagePath = nuGetCache.GetPackagePath(
                packageInfo.Id,
                packageInfo.Version.ToString());

            logger.LogDebug("Package path {packagePath}", packagePath);
            if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
            {
                logger.LogError("Package {PackageId} {Version} not found in cache",
                    packageInfo.Id, packageInfo.Version);
                throw new InvalidOperationException(
                    $"Package {packageInfo.Id} {packageInfo.Version} not found in cache");
            }

            logger.LogDebug("Creating package reader for {PackagePath}", packagePath);
            using var packageReader = new PackageArchiveReader(packagePath);

            var bestFrameworkMatch =
                await dependencyResolver.GetBestFrameworkAsync(packageInfo.Id, packageInfo.Version, packageInfo.Source);

            if (bestFrameworkMatch == null)
            {
                logger.LogError(
                    "No compatible framework found in package {PackageId} {Version}",
                    packageInfo.Id,
                    packageInfo.Version);
                throw new InvalidOperationException(
                    $"No compatible framework found in package {packageInfo.Id} {packageInfo.Version}");
            }

            logger.LogInformation("Selected best framework match: {Framework}", bestFrameworkMatch);

            Directory.CreateDirectory(extractPath);

            // Get all files from the package
            var libItems = packageReader.GetLibItems().ToList();
            var bestFrameworkFiles = libItems
                .Where(g => g.TargetFramework.Equals(bestFrameworkMatch))
                .SelectMany(g => g.Items)
                .ToList();

            if (!bestFrameworkFiles.Any())
            {
                logger.LogWarning(
                    "No library files found for framework {Framework} in package {PackageId} {Version}",
                    bestFrameworkMatch,
                    packageInfo.Id,
                    packageInfo.Version);
                return extractPath;
            }

            logger.LogDebug(
                "Found {Count} files to extract for framework {Framework}",
                bestFrameworkFiles.Count,
                bestFrameworkMatch);

            var extractedFiles = new List<string>();
            foreach (var file in bestFrameworkFiles)
                try
                {
                    var fileName = Path.GetFileName(file);
                    var destinationPath = Path.Combine(extractPath, fileName);

                    logger.LogTrace("Extracting file {File} to {Destination}", file, destinationPath);
                    using var fileStream = packageReader.GetStream(file);
                    using var destinationStream = File.Create(destinationPath);
                    await fileStream.CopyToAsync(destinationStream, cancellationToken);
                    extractedFiles.Add(destinationPath);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to extract file {File}", file);
                    throw;
                }

            logger.LogInformation(
                "Successfully extracted {FileCount} files from package {PackageId} {Version} to {Path}",
                extractedFiles.Count,
                packageInfo.Id,
                packageInfo.Version,
                extractPath);

            return extractPath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to extract package {PackageId} {Version} to {Path}",
                packageInfo.Id,
                packageInfo.Version,
                extractPath);
            throw;
        }
    }
}