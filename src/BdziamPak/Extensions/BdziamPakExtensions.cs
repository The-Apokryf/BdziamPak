using BdziamPak.Configuration;
using BdziamPak.NuGetPackages;
using BdziamPak.NuGetPackages.Dependencies;
using BdziamPak.Packages.Packaging;
using Microsoft.Extensions.DependencyInjection;

namespace BdziamPak.Extensions;

public static class BdziamPakExtensions
{
    public static IServiceCollection AddBdziamPak(this IServiceCollection services, Action<BdziamPakConfiguration> configuration)
    {
        var config = new BdziamPakConfiguration(".bdziampak");
        configuration.Invoke(config);
        
        
        services.AddSingleton<NuGetDownloadService>();
        services.AddSingleton<NuGetDependencyResolver>();
        
        
        return services.AddSingleton(sp => config);
    }
}