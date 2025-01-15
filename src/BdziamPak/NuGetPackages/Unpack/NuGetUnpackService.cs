using BdziamPak.Directory;
using BdziamPak.NuGetPackages.Cache;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Logging;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using ILogger = NuGet.Common.ILogger;

namespace BdziamPak.NuGetPackages.Unpack;

/// <summary>
/// Provides functionality for unpacking NuGet packages.
/// </summary>
/// <param name="logger">Injected logger</param>
/// <param name="nuGetCache">Injected NuGet cache</param>
/// <param name="dependencyResolver">Injected dependency resolver</param>
public class NuGetUnpackService(ILogger<NuGetUnpackService> logger, NuGetCache nuGetCache, NuGetDependencyResolver dependencyResolver)
{
    private readonly ILogger _logger = new NuGetLoggerWrapper(logger);

    /// <summary>
    /// Unpacks a NuGet package to the specified path.
    /// </summary>
    /// <param name="extractPath">The path to extract the package to.</param>
    /// <param name="packageInfo">The package information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the path where the package was extracted.</returns>
    public async Task UnpackPackageAsync(string extractPath, SourcePackageDependencyInfo packageInfo,IProgress<string> progress, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Starting package extraction for {PackageId} {Version} to {Path}", packageInfo.Id, packageInfo.Version, extractPath);

            if (string.IsNullOrEmpty(extractPath))
            {
                progress.Report("Extract path cannot be null or empty");
                logger.LogError("Extract path cannot be null or empty");
                throw new ArgumentNullException(nameof(extractPath));
            }

            if (packageInfo == null)
            {
                progress.Report("Package information cannot be null");
                logger.LogError("Package information cannot be null");
                throw new ArgumentNullException(nameof(packageInfo));
            }

            var packagePath = nuGetCache.GetPackagePath(packageInfo.Id, packageInfo.Version.ToString());

            logger.LogDebug("Package path {packagePath}", packagePath);
            if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
            {
                progress.Report($"Package {packageInfo.Id} v {packageInfo.Version} not found in cache.");
                logger.LogError("Package {PackageId} {Version} not found in cache", packageInfo.Id, packageInfo.Version);
                throw new InvalidOperationException($"Package {packageInfo.Id} {packageInfo.Version} not found in cache");
            }
            progress.Report($"Creating package reader for {packageInfo.Id}");

            logger.LogDebug("Creating package reader for {PackagePath}", packagePath);
            using var packageReader = new PackageArchiveReader(packagePath);

            var bestFrameworkMatch = await dependencyResolver.GetBestFrameworkAsync(packageInfo.Id, packageInfo.Version, packageInfo.Source);

            if (bestFrameworkMatch == null)
            {
                progress.Report($"No compatible framework found in package {packageInfo.Id} {packageInfo.Version}");

                logger.LogError("No compatible framework found in package {PackageId} {Version}", packageInfo.Id, packageInfo.Version);
                throw new InvalidOperationException($"No compatible framework found in package {packageInfo.Id} {packageInfo.Version}");
            }

            logger.LogInformation("Selected best framework match: {Framework}", bestFrameworkMatch);
            progress.Report($"Selected best framework match: {bestFrameworkMatch}");

            System.IO.Directory.CreateDirectory(extractPath);

            // Get all files from the package
            var libItems = packageReader.GetLibItems().ToList();
            var bestFrameworkFiles = libItems.Where(g => g.TargetFramework.Equals(bestFrameworkMatch)).SelectMany(g => g.Items).ToList();

            if (!bestFrameworkFiles.Any())
            {
                progress.Report($"No library files found for framework {bestFrameworkMatch.Framework} in package { packageInfo.Id} {packageInfo.Version}");

                logger.LogWarning("No library files found for framework {Framework} in package {PackageId} {Version}", bestFrameworkMatch.Framework, packageInfo.Id, packageInfo.Version);
            }
            progress.Report($"Found  { bestFrameworkFiles.Count} files to extract for framework {bestFrameworkMatch.Framework}");

            logger.LogDebug("Found {Count} files to extract for framework {Framework}", bestFrameworkFiles.Count, bestFrameworkMatch);

            var extractedFiles = new List<string>();
            foreach (var file in bestFrameworkFiles)
            {
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
            }

            logger.LogInformation("Successfully extracted {FileCount} files from package {PackageId} {Version} to {Path}", extractedFiles.Count, packageInfo.Id, packageInfo.Version, extractPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract package {PackageId} {Version} to {Path}", packageInfo.Id, packageInfo.Version, extractPath);
            throw;
        }
    }
}