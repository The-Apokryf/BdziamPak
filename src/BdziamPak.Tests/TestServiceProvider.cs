namespace BdziamPak.Tests;

using System;
using BdziamPak.Configuration;
using BdziamPak.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public static class TestServiceProvider
{
    public static IServiceProvider CreateServiceProvider(Action<BdziamPakConfiguration> configuration, Func<IServiceCollection, IServiceCollection> servicesFactory)
    {
        var services = new ServiceCollection();
        
        // Add BdziamPak services
        services.AddBdziamPak(configuration);
        servicesFactory?.Invoke(services);
        
        return services.BuildServiceProvider();
    }
}