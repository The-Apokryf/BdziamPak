using System.Text.Json;
using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BdziamPak.Tests;

public abstract class BdziamTestBase
{
    private static readonly object _fileLock = new();

    protected readonly BdziamPakMetadata _badMetadata =
        JsonSerializer.SerializeToElement(TestData.BadMetadata).Deserialize<BdziamPakMetadata>();

    protected readonly BdziamPakMetadata _goodMetadata =
        JsonSerializer.SerializeToElement(TestData.GoodMetadata).Deserialize<BdziamPakMetadata>();

    protected readonly IServiceProvider _serviceProvider;

    public BdziamTestBase(ITestOutputHelper outputHelper)
    {
        _serviceProvider = TestServiceProvider.CreateServiceProvider(
            config => { config.BdziamPakPath = ".testbdziampak"; },
            services => services.AddSingleton<ITestOutputHelper>(_ => outputHelper)
                .AddSingleton(typeof(ILogger<>), typeof(TestLogger<>)));

        var tempIndexPath = Path.Combine(
            _serviceProvider.GetRequiredService<BdziamPakDirectory>().SourcesDirectory.FullName,
            "TestSource.json");

        if (!File.Exists(tempIndexPath))
            File.WriteAllText(tempIndexPath, JsonSerializer.Serialize(TestData.SourceIndex));
    }
}