using Bdziam.ExternalDependencyResolver;
using BdziamPak.Configuration;
using BdziamPak.Git;
using BdziamPak.NuGetPackages;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.NuGetPackages.Unpack;
using BdziamPak.Packages.NuGet;
using BdziamPak.Resolving;
using BdziamPak.Structure;
using Downloader;
using Microsoft.Extensions.DependencyInjection;

namespace BdziamPak.Extensions;

public static class BdziamPakExtensions
{
    public static IServiceCollection AddBdziamPak(this IServiceCollection services,
        Action<BdziamPakConfiguration> configuration, IResolveProcessService? customResolveProcessService = null)
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
        if (customResolveProcessService != null)
            services.AddSingleton<IResolveProcessService>(customResolveProcessService);
        else
            services.AddSingleton<IResolveProcessService, ResolveProcessService>();


        services.AddTransient<BdziamPakResolveProcess>();
        services.AddExternalDependencyResolver();
        services.AddSingleton<BdziamPakService>();
        services.AddSingleton<DownloadService>(sp =>
            new DownloadService(new DownloadConfiguration { ParallelDownload = true, ParallelCount = 10 }));


        return services.AddSingleton(sp => config);
    }
}