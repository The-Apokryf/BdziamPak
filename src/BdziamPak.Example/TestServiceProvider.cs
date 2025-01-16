using BdziamPak.Configuration;
using BdziamPak.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BdziamPak.Tests;

/// <summary>
/// Provides methods to create a service provider for BdziamPak tests.
/// </summary>
public static class TestServiceProvider
{
    /// <summary>
    /// Creates a service provider with the specified configuration and services.
    /// </summary>
    /// <param name="configuration">The configuration action for BdziamPak.</param>
    /// <param name="servicesFactory">The factory function to add additional services.</param>
    /// <returns>An <see cref="IServiceProvider"/> instance with the configured services.</returns>
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