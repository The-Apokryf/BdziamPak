using System.Text.Json;
using BdziamPak.Git;
using BdziamPak.NuGetPackages;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Model;
using BdziamPak.Packaging.Install.Model;
using BdziamPak.Structure;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

public class BdziamPakService
{
    private readonly ILogger<BdziamPakService> _logger;
    private readonly Sources _sources;
    private readonly GitService _gitService;
    private readonly NuGetDownloadService _nugetDownloadService;
    private readonly NuGetDependencyResolver _dependencyResolver;
    private readonly BdziamPakDirectory _bdziamPakDirectory;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public BdziamPakService(
        ILogger<BdziamPakService> logger,
        Sources sources,
        GitService gitService,
        NuGetDownloadService nugetDownloadService,
        NuGetDependencyResolver dependencyResolver,
        BdziamPakDirectory bdziamPakDirectory)
    {
        _logger = logger;
        _sources = sources;
        _gitService = gitService;
        _nugetDownloadService = nugetDownloadService;
        _dependencyResolver = dependencyResolver;
        _bdziamPakDirectory = bdziamPakDirectory;
    }

    private async Task<List<LocalBdziamPak>> LoadPaksJsonAsync()
    {
        var paksJsonPath = Path.Combine(_bdziamPakDirectory.PaksDirectory.FullName, "Paks.json");
        if (!File.Exists(paksJsonPath))
            return new List<LocalBdziamPak>();

        var json = await File.ReadAllTextAsync(paksJsonPath);
        return JsonSerializer.Deserialize<List<LocalBdziamPak>>(json) ?? new List<LocalBdziamPak>();
    }

    private async Task SavePaksJsonAsync(List<LocalBdziamPak> paks)
    {
        var paksJsonPath = Path.Combine(_bdziamPakDirectory.PaksDirectory.FullName, "Paks.json");
        var json = JsonSerializer.Serialize(paks, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(paksJsonPath, json);
    }

    public async Task<List<LocalBdziamPak>> ListInstalled()
    {
        return await LoadPaksJsonAsync();
    }

    public async Task<bool> IsDependency(string bdziamPakId, string version)
    {
        var installedPaks = await LoadPaksJsonAsync();
        return installedPaks.Any(pak => 
            pak.Dependencies?.Any(dep => 
                dep.BdziamPakId == bdziamPakId && dep.Version == version) ?? false);
    }

    public async Task<bool> IsResolved(string bdziamPakId)
    {
        var installedPaks = await LoadPaksJsonAsync();
        return installedPaks.Any(pak => pak.BdziamPakId == bdziamPakId);
    }

    public async Task<List<LocalBdziamPak>> CheckForUpdates()
    {
        var installedPaks = await LoadPaksJsonAsync();
        var updates = new List<LocalBdziamPak>();

        foreach (var pak in installedPaks)
        {
            var searchResults = await _sources.SearchAsync(pak.BdziamPakId);
            var latestVersion = searchResults
                .Where(r => r.GetValueOrDefault().Package.BdziamPakId == pak.BdziamPakId)
                .MaxBy(r => Version.Parse(r.GetValueOrDefault().Package.Version))?.Package;

            if (latestVersion != null && Version.Parse(latestVersion.Version) > Version.Parse(pak.Version))
            {
                updates.Add(new LocalBdziamPak 
                { 
                    BdziamPakId = pak.BdziamPakId,
                    Version = latestVersion.Version
                });
            }
        }

        return updates;
    }

    public async Task Upgrade()
    {
        var updates = await CheckForUpdates();
        foreach (var update in updates)
        {
            if (!await IsDependency(update.BdziamPakId, update.Version))
            {
                var progress = new Progress<BdziamPakInstallProgress>();
                await ResolveBdziamPakAsync(update.BdziamPakId, update.Version, progress);
            }
        }
    }

    public async Task<BdziamPakInstallResult> ResolveBdziamPakAsync(
        string bdziamPakId,
        string version,
        IProgress<BdziamPakInstallProgress> progress)
    {
        try
        {
            await _semaphore.WaitAsync();
            var result = new BdziamPakInstallResult();
            var progressData = new BdziamPakInstallProgress
            {
                Message = $"Starting resolution of {bdziamPakId} v{version}"
            };

            _logger.LogInformation("Starting resolution of {BdziamPakId} v{Version}", bdziamPakId, version);
            progress.Report(progressData);

            // Search for the package in sources
            var searchResults = await _sources.SearchAsync(bdziamPakId);
            var metadata = searchResults
                .FirstOrDefault(r => r.GetValueOrDefault().Package.BdziamPakId == bdziamPakId && r.GetValueOrDefault().Package.Version == version)
                ?.Package;

            if (metadata == null)
            {
                result.Message = $"Package {bdziamPakId} v{version} not found in any source";
                return result;
            }

            // Resolve dependencies first
            var resolvedDependencies = new List<LocalBdziamPak>();
            if (metadata.BdziamPakDependencies?.Any() == true)
            {
                foreach (var dep in metadata.BdziamPakDependencies)
                {
                    var depProgress = new Progress<BdziamPakInstallProgress>();
                    progressData.DependencyProgresses.Add(depProgress);
                    progress.Report(progressData);

                    var depResult = await ResolveBdziamPakAsync(dep.BdziamPakId, dep.Version, depProgress);
                    resolvedDependencies.AddRange(depResult.ResolvedDependencies);
                }
            }

            // Create package directory
            var pakDirectory = new DirectoryInfo(Path.Combine(
                _bdziamPakDirectory.PaksDirectory.FullName,
                $"{bdziamPakId}@{version}"));

            // Clone repository or create directory
            if (metadata.Repository != null)
            {
                progressData.Message = "Cloning repository...";
                progress.Report(progressData);
                _gitService.CloneRepo(metadata);
            }
            else
            {
                pakDirectory.Create();
            }

            // Process NuGet packages
            var libDirectory = new DirectoryInfo(Path.Combine(pakDirectory.FullName, "Lib"));
            libDirectory.Create();

            if (metadata.Repository != null)
            {
                progressData.Message = "Resolving NuGet dependencies...";
                progress.Report(progressData);

                var settings = Settings.LoadDefaultSettings(null);
                var repository = new SourceRepositoryProvider(
                    new PackageSourceProvider(settings),
                    Repository.Provider.GetCoreV3())
                    .GetRepositories()
                    .First();

                var packages = await _dependencyResolver.LoadPackageDependenciesAsync(
                    metadata.NuGetPackage.PackageId,
                    NuGetVersion.Parse(metadata.NuGetPackage.PackageVersion),
                    repository);

                progressData.TotalPackages = packages.Count();
                progress.Report(progressData);

                var downloadTasks = packages.Select(async package =>
                {
                    var nugetProgress = new Progress<NuGetDownloadProgress>();
                    progressData.NuGetProgresses.Add(nugetProgress);
                    progress.Report(progressData);

                    await _nugetDownloadService.DownloadPackageAsync(
                        package.Id,
                        package.Version.ToString(),
                        libDirectory.FullName,
                        nugetProgress);

                    progressData.CompletedPackages++;
                    progress.Report(progressData);
                });

                await Task.WhenAll(downloadTasks);
            }

            // Update Paks.json
            var installedPaks = await LoadPaksJsonAsync();
            installedPaks.RemoveAll(p => p.BdziamPakId == bdziamPakId);
            installedPaks.Add(new LocalBdziamPak
            {
                BdziamPakId = bdziamPakId,
                Version = version,
                Dependencies = metadata.BdziamPakDependencies?.Select(d => new LocalBdziamPak
                {
                    BdziamPakId = d.BdziamPakId,
                    Version = d.Version
                }).ToArray() ?? []
            });

            await SavePaksJsonAsync(installedPaks);

            result.Success = true;
            result.Message = $"Successfully resolved {bdziamPakId} v{version}";
            result.ResolvedDependencies = resolvedDependencies;
            result.TotalResolved = resolvedDependencies.Count + 1;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve BdziamPak {BdziamPakId} v{Version}", bdziamPakId, version);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}