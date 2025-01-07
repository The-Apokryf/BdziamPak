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
    protected readonly IServiceProvider _serviceProvider;
    protected readonly BdziamPakMetadata _goodMetadata = TestData.GoodMetadata;
    protected readonly BdziamPakMetadata _badMetadata = TestData.BadMetadata;

    public BdziamTestBase(ITestOutputHelper outputHelper)
    {
        _serviceProvider = TestServiceProvider.CreateServiceProvider(config =>
        {
            config.BdziamPakPath = ".testbdziampak";
        }, services => services.AddSingleton<ITestOutputHelper>(_ => outputHelper).AddSingleton(typeof(ILogger<>), typeof(TestLogger<>)));

        var tempIndexPath = Path.Combine(_serviceProvider.GetRequiredService<BdziamPakDirectory>().SourcesDirectory.FullName, 
            $"{TestData.SourceIndex.Name}.json");
        
        if(!File.Exists(tempIndexPath))
            File.WriteAllText(tempIndexPath, JsonSerializer.Serialize(TestData.SourceIndex));
    }
}