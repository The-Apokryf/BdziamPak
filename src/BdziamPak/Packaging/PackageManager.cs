using System.Text.Json;
using System.Text.RegularExpressions;
using BdziamPak.Packages.Index.Local;
using BdziamPak.Packages.Index.Model;

namespace BdziamPak.Packages.Packaging;

public class PackageManager(NugetService nugetService, )
{
    
    public async Task<List<Model.BdziamPakMetadata>> ListPackages(string? source = null, string? searchString = null, PackageListMode packageListMode = PackageListMode.All)
    {
            return (await GetPackages())
            .Where(p => string.IsNullOrEmpty(searchString) || Regex.Match(p.PackageId.ToString(), searchString).Success)
            .ToList();
    }

    private async Task<IQueryable<Model.BdziamPakMetadata>> GetPackages()
    {
        var sources = await localIndexMaintainer.GetSources();
        return sources
            .SelectMany(x => x.).AsQueryable();
    }

    private async Task<IQueryable<Model.BdziamPakMetadata>> GetInstalledPackages()
    {
        
    }
    
    public async Task DownloadPackage(string package)
    {
        var pak = _sourceService.(sourceName, packageId);

        await nugetService.DownloadPackageAsync(pak.PackageId, pak.Version, pak.NugetFeedUrl);
    }

    public void RemovePackage(string package)
    {
        // Locate and remove the package
    }

    public async Task UpgradePackages(string? source = null)
    {
        var packages = await ListPackages(source);
        foreach (var package in packages)
        {
            await DownloadPackage($"{package.PackageId}@{package.Version}");
        }
    }

    private (string source, string packageId) ParsePackage(string package)
    {
        var parts = package.Split('.');
        return (parts[0], parts[1]);
    }
}
