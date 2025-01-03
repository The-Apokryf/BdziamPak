using BdziamPak.NuGetPackages.Logging;
using BdziamPak.NuGetPackages.Model;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace BdziamPak.NuGetPackages;

public class NuGetDownloadService(ILogger<NuGetDownloadService> logger)
{
    private readonly NuGet.Common.ILogger _logger = new NuGetLoggerWrapper(logger);

    public async Task<IPackageSearchMetadata?> SearchPackageAsync(string packageId, string version)
    {
        try
        {
            logger.LogDebug("Starting package search for {PackageId} version {Version}", packageId, version);

            var settings = Settings.LoadDefaultSettings(root: null);
            logger.LogTrace("NuGet settings loaded from default location");

            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings), 
                Repository.Provider.GetCoreV3());
            logger.LogDebug("Created SourceRepositoryProvider with CoreV3");

            var repository = sourceRepositoryProvider.CreateRepository(
                new PackageSource("https://api.nuget.org/v3/index.json"));
            logger.LogDebug("Created repository for nuget.org source");

            logger.LogTrace("Retrieving PackageMetadataResource");
            var resource = await repository.GetResourceAsync<PackageMetadataResource>();
            logger.LogDebug("PackageMetadataResource retrieved successfully");

            logger.LogInformation("Searching for package {PackageId} {Version}", packageId, version);
            var searchMetadata = await resource.GetMetadataAsync(
                packageId,
                includePrerelease: true,
                includeUnlisted: false,
                new SourceCacheContext(),
                _logger,
                CancellationToken.None);

            var result = searchMetadata.FirstOrDefault(x => x.Identity.Version.ToString() == version);
            logger.LogInformation(
                result != null 
                    ? "Package {PackageId} {Version} found" 
                    : "Package {PackageId} {Version} not found", 
                packageId, 
                version);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching for package {PackageId} {Version}", packageId, version);
            throw;
        }
    }

    public async Task DownloadPackageAsync(
        string packageId, 
        string version, 
        string downloadPath,
        IProgress<NuGetDownloadProgress> progress)
    {
        try
        {
            logger.LogDebug("Starting package download for {PackageId} {Version} to {DownloadPath}", 
                packageId, version, downloadPath);

            var settings = Settings.LoadDefaultSettings(root: null);
            logger.LogTrace("NuGet settings loaded from default location");

            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings), 
                Repository.Provider.GetCoreV3());
            logger.LogDebug("Created SourceRepositoryProvider with CoreV3");

            var repository = sourceRepositoryProvider.CreateRepository(
                new PackageSource("https://api.nuget.org/v3/index.json"));
            logger.LogDebug("Created repository for nuget.org source");

            logger.LogTrace("Retrieving DownloadResource");
            var downloadResource = await repository.GetResourceAsync<DownloadResource>();
            logger.LogDebug("DownloadResource retrieved successfully");

            var packageIdentity = new PackageIdentity(packageId, new NuGetVersion(version));
            var context = new PackageDownloadContext(new SourceCacheContext());
            
            progress.Report(new NuGetDownloadProgress("Starting download..."));
            logger.LogInformation("Initiating download for package {PackageId} {Version}", packageId, version);

            var downloadResult = await downloadResource.GetDownloadResourceResultAsync(
                packageIdentity,
                context,
                downloadPath,
                _logger,
                CancellationToken.None);

            if (downloadResult.Status == DownloadResourceResultStatus.Available)
            {
                var packageFilePath = Path.Combine(downloadPath, $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg");
                logger.LogDebug("Package will be saved to {PackageFilePath}", packageFilePath);

                using (var fileStream = new FileStream(packageFilePath, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[81920]; // 80 KB buffer
                    long totalBytesRead = 0;
                    int bytesRead;
                    var totalLength = downloadResult.PackageStream.Length;

                    logger.LogDebug("Starting to write package stream, total size: {TotalBytes} bytes", totalLength);

                    while ((bytesRead = await downloadResult.PackageStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        var percentComplete = (int)((totalBytesRead * 100) / totalLength);
                        logger.LogTrace("Download progress: {BytesRead}/{TotalBytes} bytes ({PercentComplete}%)", 
                            totalBytesRead, totalLength, percentComplete);
                        
                        progress.Report(new NuGetDownloadProgress(
                            $"Downloading... {totalBytesRead}/{totalLength} bytes", 
                            percentComplete));
                    }
                }

                logger.LogInformation("Package {PackageId} {Version} downloaded successfully", packageId, version);
                progress.Report(new NuGetDownloadProgress(
                    $"Package {packageId} {version} downloaded successfully.", 
                    100));
            }
            else
            {
                logger.LogWarning("Package not found: {PackageId} {Version}", packageId, version);
                progress.Report(new NuGetDownloadProgress($"Package not found: {packageId} {version}"));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading package {PackageId} {Version}", packageId, version);
            progress.Report(new NuGetDownloadProgress($"Error downloading package: {ex.Message}"));
            throw;
        }
    }
}