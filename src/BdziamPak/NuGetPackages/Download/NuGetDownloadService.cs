using System.Reflection;
using BdziamPak.NuGetPackages.Logging;
using BdziamPak.NuGetPackages.Model;
using BdziamPak.Packages.NuGet;
using Downloader;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ILogger = NuGet.Common.ILogger;

namespace BdziamPak.NuGetPackages;

/// <summary>
/// Provides functionality for downloading NuGet packages.
/// </summary>
public class NuGetDownloadService
{
    private readonly DownloadService _downloader;
    private readonly ILogger<NuGetDownloadService> _logger;
    private readonly ILogger _nugetLogger;
    private NuGetDownloadProgress? _progress;

    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetDownloadService"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="cache">The NuGet cache.</param>
    /// <param name="downloader">The download service.</param>
    public NuGetDownloadService(ILogger<NuGetDownloadService> logger, NuGetCache cache, DownloadService downloader)
    {
        _logger = logger;
        _downloader = downloader;
        _nugetLogger = new NuGetLoggerWrapper(logger);
    }

    /// <summary>
    /// Searches for a NuGet package by its ID and version.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the package metadata if found, otherwise null.</returns>
    public async Task<IPackageSearchMetadata?> SearchPackageAsync(string packageId, string version)
    {
        try
        {
            _logger.LogDebug("Starting package search for {PackageId} version {Version}", packageId, version);

            var settings = Settings.LoadDefaultSettings(null);
            _logger.LogTrace("NuGet settings loaded from default location");

            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings),
                Repository.Provider.GetCoreV3());
            _logger.LogDebug("Created SourceRepositoryProvider with CoreV3");

            var repository = sourceRepositoryProvider.CreateRepository(
                new PackageSource("https://api.nuget.org/v3/index.json"));
            _logger.LogDebug("Created repository for nuget.org source");

            _logger.LogTrace("Retrieving PackageMetadataResource");
            var resource = await repository.GetResourceAsync<PackageMetadataResource>();
            _logger.LogDebug("PackageMetadataResource retrieved successfully");

            _logger.LogInformation("Searching for package {PackageId} {Version}", packageId, version);
            var searchMetadata = await resource.GetMetadataAsync(
                packageId,
                true,
                false,
                new SourceCacheContext(),
                _nugetLogger,
                CancellationToken.None);

            var result = searchMetadata.FirstOrDefault(x => x.Identity.Version.ToString() == version);
            _logger.LogInformation(
                result != null
                    ? "Package {PackageId} {Version} found"
                    : "Package {PackageId} {Version} not found",
                packageId,
                version);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for package {PackageId} {Version}", packageId, version);
            throw;
        }
    }

    /// <summary>
    /// Downloads a NuGet package to the specified path.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <param name="downloadPath">The path to download the package to.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DownloadPackageAsync(
        string packageId,
        string version,
        string downloadPath,
        IProgress<NuGetDownloadProgress> progress)
    {
        try
        {
            _logger.LogDebug("Starting package download for {PackageId} {Version} to {DownloadPath}",
                packageId, version, downloadPath);

            var settings = Settings.LoadDefaultSettings(null);
            _logger.LogTrace("NuGet settings loaded from default location");

            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings),
                Repository.Provider.GetCoreV3());
            _logger.LogDebug("Created SourceRepositoryProvider with CoreV3");

            var repository = sourceRepositoryProvider.CreateRepository(
                new PackageSource("https://api.nuget.org/v3/index.json"));
            _logger.LogDebug("Created repository for nuget.org source");

            _logger.LogTrace("Retrieving DownloadResource");
            var downloadResource = await repository.GetResourceAsync<DownloadResource>();
            _logger.LogDebug("DownloadResource retrieved successfully");

            var packageIdentity = new PackageIdentity(packageId, new NuGetVersion(version));
            progress.Report(new NuGetDownloadProgress("Starting download..."));
            _logger.LogInformation("Initiating download for package {PackageId} {Version}", packageId, version);

            if (downloadResource is DownloadResourceV3 downloadResourceV3)
            {
                var file = Path.Combine(downloadPath, $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg");

                var downloadUrlMethod = downloadResourceV3
                    .GetType()
                    .GetMethod("GetDownloadUrl", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(downloadResourceV3,
                        new object[] { packageIdentity, _nugetLogger, CancellationToken.None }) as Task<Uri>;

                var downloadUrl = await downloadUrlMethod!;
                _downloader.DownloadStarted += (sender, args) =>
                {
                    _progress = new NuGetDownloadProgress(
                        $"Downloading... {args.FileName} {args.TotalBytesToReceive} bytes",
                        0);
                    progress.Report(_progress);
                    _logger.LogInformation("Download started: {FileName} {TotalBytes}", args.FileName,
                        args.TotalBytesToReceive);
                };
                _downloader.DownloadProgressChanged += (sender, args) =>
                {
                    _progress = new NuGetDownloadProgress(
                        $"Downloading... {args.ReceivedBytesSize}/{args.TotalBytesToReceive} bytes",
                        (int)(args.ReceivedBytesSize * 100 / args.TotalBytesToReceive));
                    _logger.LogInformation("Download progress: {Progress}", _progress);
                };

                _downloader.DownloadFileCompleted += (sender, args) =>
                {
                    _progress = new NuGetDownloadProgress("Download completed", 100);
                    progress.Report(_progress);
                    if (args.Error != null)
                    {
                        _logger.LogError(args.Error, "Error occured while downloading resouce {resource}", file);
                        throw new Exception(args.Error.Message);
                    }

                    _logger.LogInformation("Download completed for {item}", file);
                };
                await _downloader.DownloadFileTaskAsync(downloadUrl.AbsoluteUri, file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading package {PackageId} {Version}", packageId, version);
            progress.Report(new NuGetDownloadProgress($"Error downloading package: {ex.Message}"));
            throw;
        }
    }
}