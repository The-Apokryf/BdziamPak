using BdziamPak.Configuration;
using BdziamPak.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BdziamPak.Tests;

public static class TestServiceProvider
{
    public static IServiceProvider CreateServiceProvider(Action<BdziamPakConfiguration> configuration,
        Func<IServiceCollection, IServiceCollection> servicesFactory)
    {
        var services = new ServiceCollection();

        // Add BdziamPak services
        services.AddBdziamPak(configuration);
        servicesFactory?.Invoke(services);

        return services.BuildServiceProvider();
    }
}