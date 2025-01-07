using BdziamPak.Configuration;
using BdziamPak.Git;
using BdziamPak.NuGetPackages;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Unpack;
using BdziamPak.Packages.NuGet;
using BdziamPak.Packages.Packaging;
using BdziamPak.Structure;
using Downloader;
using Microsoft.Extensions.DependencyInjection;

namespace BdziamPak.Extensions;

public static class BdziamPakExtensions
{
    public static IServiceCollection AddBdziamPak(this IServiceCollection services, Action<BdziamPakConfiguration> configuration)
    {
        var config = new BdziamPakConfiguration(".bdziampak");
        configuration.Invoke(config);
        
        
        services.AddSingleton<BdziamPakDirectory>();
        services.AddSingleton<NuGetCache>();
        services.AddSingleton<GitCredentials>();
        services.AddSingleton<GitService>();
        services.AddSingleton<NuGetDownloadService>();
        services.AddSingleton<NuGetDependencyResolver>();
        services.AddSingleton<NuGetUnpackService>();
        services.AddSingleton<Sources>();
        services.AddSingleton<BdziamPakService>();
        services.AddSingleton<DownloadService>(sp => new DownloadService(new DownloadConfiguration(){ParallelDownload = true,ParallelCount = 10}));
        
        
        return services.AddSingleton(sp => config);
    }
}