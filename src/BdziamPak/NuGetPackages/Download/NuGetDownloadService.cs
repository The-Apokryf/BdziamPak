using System.Reflection;
using BdziamPak.NuGetPackages.Cache;
using BdziamPak.NuGetPackages.Logging;
using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using Downloader;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ILogger = NuGet.Common.ILogger;

namespace BdziamPak.NuGetPackages.Download;

/// <summary>
///     Provides functionality for downloading NuGet packages.
/// </summary>
public class NuGetDownloadService
{
    private readonly DownloadService _downloader;
    private readonly ILogger<NuGetDownloadService> _logger;
    private readonly ILogger _nugetLogger;
    /// <summary>
    ///     Initializes a new instance of the <see cref="NuGetDownloadService" /> class.
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
    ///     Searches for a NuGet package by its ID and version.
    /// </summary>
    /// <param name="packageId">The ID of the package.</param>
    /// <param name="version">The version of the package.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the package metadata if found,
    ///     otherwise null.
    /// </returns>
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
    ///     Downloads a NuGet package to the specified path.
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
        StepProgress progress)
    {
        try
        {
            progress.UpdateAndReport($"Initializing package download for {packageId}.{version} to {downloadPath}");
            var settings = Settings.LoadDefaultSettings(null);
            _logger.LogTrace("NuGet settings loaded from default location");

            var sourceRepositoryProvider = new SourceRepositoryProvider(
                new PackageSourceProvider(settings),
                Repository.Provider.GetCoreV3());
            _logger.LogDebug("Created SourceRepositoryProvider with CoreV3");

            var repository = sourceRepositoryProvider.CreateRepository(
                new PackageSource("https://api.nuget.org/v3/index.json"));
            _logger.LogDebug("Created repository for nuget.org source");
            progress.Info("Main Package", $"{packageId}.{version}");
            _logger.LogTrace("Retrieving DownloadResource");
            var downloadResource = await repository.GetResourceAsync<DownloadResource>();
            _logger.LogDebug("DownloadResource retrieved successfully");

            var packageIdentity = new PackageIdentity(packageId, new NuGetVersion(version));

            _logger.LogInformation("Initiating download for package {PackageId} {Version}", packageId, version);

            if (downloadResource is DownloadResourceV3 downloadResourceV3)
            {
                var file = Path.Combine(downloadPath, $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg");

                var downloadUrlMethod = downloadResourceV3
                    .GetType()
                    .GetMethod("GetDownloadUrl", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(downloadResourceV3,
                        new object[] { packageIdentity, _nugetLogger, CancellationToken.None }) as Task<Uri>;
                var progressName = $"Download {packageId}.{version}";
                var downloadUrl = await downloadUrlMethod!;
                _downloader.DownloadStarted += (sender, args) =>
                {
                    progress.Determinate(progressName, 0, (int)args.TotalBytesToReceive);
                    progress.Info(progressName, $"Downloading {args.FileName}...");
                    _logger.LogInformation("Download started: {FileName} {TotalBytes}", args.FileName,
                        args.TotalBytesToReceive);
                };
                _downloader.DownloadProgressChanged += (sender, args) =>
                {
                    progress.Determinate(progressName, (int)args.ReceivedBytesSize,
                        (int)(args.TotalBytesToReceive));
                    progress.Status(
                        $"Received {(int)args.ReceivedBytesSize} out of {(int)args.TotalBytesToReceive} bytes");
                };

                _downloader.DownloadFileCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        _logger.LogError(args.Error, "Error occured while downloading resource {resource}", file);
                        progress.UpdateAndReport(
                            $"Error occured while downloading resource {file}: {args.Error.Message}", StepState.Failed);
                        progress.Finish(progressName, true);
                        return;
                    }
                    progress.FinishIndicator(progressName);
                    _logger.LogInformation("Download completed for {item}", file);
                };
                await _downloader.DownloadFileTaskAsync(downloadUrl.AbsoluteUri, file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading package {PackageId} {Version}", packageId, version);
            progress.Finish($"Error occured while executing step: {ex.Message}", true);
            throw;
        }
    }
}