using BdziamPak.Packaging.Install.Model;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace BdziamPak.Tests;

public class BdziamPakServiceTests(ITestOutputHelper outputHelper) : BdziamTestBase(outputHelper)
{
    [Fact]
    public async Task ResolveTest()
    {
        var bdziamPakService = _serviceProvider.GetRequiredService<BdziamPakService>();
        
        // Act
        var progress = new Progress<BdziamPakInstallProgress>();
        progress.ProgressChanged +=
            (p, e) => outputHelper.WriteLine("{0}, Progress: {1}", e.ProgressPercentage, e.Message);
        var result = await bdziamPakService.ResolveBdziamPakAsync(_goodMetadata.BdziamPakId, _goodMetadata.Version, progress);
        // Assert
        Assert.NotNull(result);
    }
}