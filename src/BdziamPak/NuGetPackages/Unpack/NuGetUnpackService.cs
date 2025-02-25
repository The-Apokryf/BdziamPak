﻿using BdziamPak.NuGetPackages.Cache;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Logging;
using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using ILogger = NuGet.Common.ILogger;

namespace BdziamPak.NuGetPackages.Unpack;

/// <summary>
///     Provides functionality for unpacking NuGet packages.
/// </summary>
/// <param name="logger">Injected logger</param>
/// <param name="nuGetCache">Injected NuGet cache</param>
/// <param name="dependencyResolver">Injected dependency resolver</param>
public class NuGetUnpackService(
    ILogger<NuGetUnpackService> logger,
    NuGetCache nuGetCache,
    NuGetDependencyResolver dependencyResolver)
{
    private readonly ILogger _logger = new NuGetLoggerWrapper(logger);

    /// <summary>
    ///     Unpacks a NuGet package to the specified path.
    /// </summary>
    /// <param name="extractPath">The path to extract the package to.</param>
    /// <param name="packageInfo">The package information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the path where the package was
    ///     extracted.
    /// </returns>
    public async Task UnpackPackageAsync(string extractPath, SourcePackageDependencyInfo packageInfo,
        StepProgress stepProgress, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Starting package extraction for {PackageId} {Version} to {Path}", packageInfo.Id,
                packageInfo.Version, extractPath);

            if (string.IsNullOrEmpty(extractPath))
            {
                stepProgress.UpdateAndReport($"Extract path ({extractPath}) cannot be null or empty", StepState.Failed);
                logger.LogError("Extract path cannot be null or empty");
                throw new ArgumentNullException(nameof(extractPath));
            }

            if (packageInfo == null)
            {
                stepProgress.UpdateAndReport("Package info cannot be null", StepState.Failed);
                logger.LogError("Package information cannot be null");
                throw new ArgumentNullException(nameof(packageInfo));
            }

            var packagePath = nuGetCache.GetPackagePath(packageInfo.Id, packageInfo.Version.ToString());

            logger.LogDebug("Package path {packagePath}", packagePath);
            if (string.IsNullOrEmpty(packagePath) || !File.Exists(packagePath))
            {
                stepProgress.UpdateAndReport("Package info cannot be null", StepState.Failed);
                logger.LogError("Package {PackageId} {Version} not found in cache", packageInfo.Id,
                    packageInfo.Version);
                throw new InvalidOperationException(
                    $"Package {packageInfo.Id} {packageInfo.Version} not found in cache");
            }

            logger.LogDebug("Creating package reader for {PackagePath}", packagePath);
            using var packageReader = new PackageArchiveReader(packagePath);

            var bestFrameworkMatch =
                await dependencyResolver.GetBestFrameworkAsync(packageInfo.Id, packageInfo.Version, packageInfo.Source);

            if (bestFrameworkMatch == null)
            {
                stepProgress.UpdateAndReport(
                    $"No compatible framework found in package {packageInfo.Id} {packageInfo.Version}",
                    StepState.Failed);
                logger.LogError("No compatible framework found in package {PackageId} {Version}", packageInfo.Id,
                    packageInfo.Version);
                throw new InvalidOperationException(
                    $"No compatible framework found in package {packageInfo.Id} {packageInfo.Version}");
            }

            logger.LogInformation("Selected best framework match: {Framework}", bestFrameworkMatch);
            stepProgress.Indeterminate("Unpacking: ",
                $"{packageInfo.Id}.{packageInfo.Version} (target: {bestFrameworkMatch})");

            System.IO.Directory.CreateDirectory(extractPath);

            // Get all files from the package
            var libItems = packageReader.GetLibItems().ToList();
            var bestFrameworkFiles = libItems.Where(g => g.TargetFramework.Equals(bestFrameworkMatch))
                .SelectMany(g => g.Items).ToList();

            if (!bestFrameworkFiles.Any())
            {
                stepProgress.UpdateAndReport("No library files found", StepState.Failed);
                logger.LogWarning("No library files found for framework {Framework} in package {PackageId} {Version}",
                    bestFrameworkMatch.Framework, packageInfo.Id, packageInfo.Version);
            }

            var unpackProgressName = $"Unpack {packageInfo.Id}.{packageInfo.Version}";
            stepProgress.Determinate(unpackProgressName, 0, bestFrameworkFiles.Count);
            stepProgress.Status($"Found  {bestFrameworkFiles.Count} files to extract for framework {bestFrameworkMatch.Framework}");

            logger.LogDebug("Found {Count} files to extract for framework {Framework}", bestFrameworkFiles.Count,
                bestFrameworkMatch);

            var extractedFiles = new List<string>();
            foreach (var file in bestFrameworkFiles)
                try
                {
                    var fileName = Path.GetFileName(file);
                    var destinationPath = Path.Combine(extractPath, fileName);
                    var currentFile = bestFrameworkFiles.IndexOf(file) + 1;
                    stepProgress.Determinate(unpackProgressName, currentFile, bestFrameworkFiles.Count);
                    stepProgress.Status($"({currentFile}/{bestFrameworkFiles.Count}) Extracting {fileName}...");
                    logger.LogTrace("Extracting file {File} to {Destination}", file, destinationPath);
                    using var fileStream = packageReader.GetStream(file);
                    using var destinationStream = File.Create(destinationPath);
                    await fileStream.CopyToAsync(destinationStream, cancellationToken);
                    extractedFiles.Add(destinationPath);
                }
                catch (Exception ex)
                {
                    stepProgress.UpdateAndReport($"Failed to extract file {file}: {ex.Message}", StepState.Failed);
                    logger.LogError(ex, "Failed to extract file {File}", file);
                    throw;
                }

            stepProgress.UpdateAndReport(
                $"Successfully extracted {extractedFiles.Count} files from package {packageInfo.Id}.{packageInfo.Version} to {extractPath}");
            logger.LogInformation(
                "Successfully extracted {FileCount} files from package {PackageId} {Version} to {Path}",
                extractedFiles.Count, packageInfo.Id, packageInfo.Version, extractPath);
        }
        catch (Exception ex)
        {
            stepProgress.Finish(
                $"Failed to extract package {packageInfo.Id}.{packageInfo.Version} to {extractPath}", true);
            logger.LogError(ex, "Failed to extract package {PackageId} {Version} to {Path}", packageInfo.Id,
                packageInfo.Version, extractPath);
            throw;
        }
    }
}