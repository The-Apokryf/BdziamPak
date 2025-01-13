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

/// <summary>
/// Provides extension methods for configuring BdziamPak services.
/// </summary>
public static class BdziamPakExtensions
{
    /// <summary>
    /// Adds BdziamPak services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configuration">The configuration action to configure <see cref="BdziamPakConfiguration"/>.</param>
    /// <param name="customResolveProcessService">An optional custom resolve process service.</param>
    /// <returns>The service collection with the added services.</returns>
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

        services.AddTransient<BdziamPakProcess>();
        services.AddExternalDependencyResolver();
        services.AddSingleton<BdziamPakService>();
        services.AddSingleton<DownloadService>(sp =>
            new DownloadService(new DownloadConfiguration { ParallelDownload = true, ParallelCount = 10 }));

        return services.AddSingleton(sp => config);
    }
}