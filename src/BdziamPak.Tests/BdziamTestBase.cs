using System.Text.Json;
using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BdziamPak.Tests;

/// <summary>
/// Base class for BdziamPak unit tests.
/// </summary>
public abstract class BdziamTestBase
{
    private static readonly object _fileLock = new();

    /// <summary>
    /// Metadata representing a bad BdziamPak.
    /// </summary>
    protected readonly BdziamPakMetadata _badMetadata =
        JsonSerializer.SerializeToElement(TestData.BadMetadata).Deserialize<BdziamPakMetadata>();

    /// <summary>
    /// Metadata representing a good BdziamPak.
    /// </summary>
    protected readonly BdziamPakMetadata _goodMetadata =
        JsonSerializer.SerializeToElement(TestData.GoodMetadata).Deserialize<BdziamPakMetadata>();

    /// <summary>
    /// Service provider for dependency injection.
    /// </summary>
    protected readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BdziamTestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The output helper for test output.</param>
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