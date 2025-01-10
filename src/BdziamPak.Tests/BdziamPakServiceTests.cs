using BdziamPak.Packaging.Install.Model;
using BdziamPak.Resolving;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace BdziamPak.Tests;

/// <summary>
/// Unit tests for the BdziamPakService.
/// </summary>
public class BdziamPakServiceTests(ITestOutputHelper outputHelper) : BdziamTestBase(outputHelper)
{
    /// <summary>
    /// Tests the ResolveBdziamPakAsync method of BdziamPakService.
    /// </summary>
    [Fact]
    public async Task  ResolveBdziamPakAsync_WithValidMetadata_ReturnsAnyResult()
    {
        var bdziamPakService = _serviceProvider.GetRequiredService<BdziamPakService>();

        // Act
        var progress = new Progress<BdziamPakResolveProgress>();
        progress.ProgressChanged +=
            (p, e) => outputHelper.WriteLine("{0}, Progress: {1}", e.Percent, e.Message);
        var result =
            await bdziamPakService.ResolveBdziamPakAsync(_goodMetadata.BdziamPakId, _goodMetadata.Version, progress);

        // Assert
        Assert.NotNull(result);
    }

    /// <summary>
    /// Resolves a BdziamPak package with valid metadata.
    /// </summary>
    [Fact]
    public async Task ResolveBdziamPakAsync_WithValidMetadata_ReturnsSuccessResult()
    {
        var bdziamPakService = _serviceProvider.GetRequiredService<BdziamPakService>();

        var progress = new Progress<BdziamPakResolveProgress>();
        progress.ProgressChanged += (p, e) => outputHelper.WriteLine("{0}, Progress: {1}", e.Percent, e.Message);
        var result = await bdziamPakService.ResolveBdziamPakAsync("TestAuthor.testPak", "1.0.0", progress);

        Assert.True(result.Success);
        Assert.Equal("Successfully resolved TestAuthor.testPak v1.0.0", result.Message);
    }

    /// <summary>
    /// Resolves a BdziamPak package with invalid metadata.
    /// </summary>
    [Fact]
    public async Task ResolveBdziamPakAsync_WithInvalidMetadata_ReturnsFailureResult()
    {
        var bdziamPakService = _serviceProvider.GetRequiredService<BdziamPakService>();

        var progress = new Progress<BdziamPakResolveProgress>();
        progress.ProgressChanged += (p, e) => outputHelper.WriteLine("{0}, Progress: {1}", e.Percent, e.Message);
        var result = await bdziamPakService.ResolveBdziamPakAsync("invalidId", "1.0.0", progress);

        Assert.False(result.Success);
        Assert.Equal("Package invalidId v1.0.0 not found", result.Message);
    }

    /// <summary>
    /// Resolves a BdziamPak package with null metadata.
    /// </summary>
    [Fact]
    public async Task ResolveBdziamPakAsync_WithNullMetadata_ThrowsArgumentNullException()
    {
        var bdziamPakService = _serviceProvider.GetRequiredService<BdziamPakService>();

        var progress = new Progress<BdziamPakResolveProgress>();
        progress.ProgressChanged += (p, e) => outputHelper.WriteLine("{0}, Progress: {1}", e.Percent, e.Message);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await bdziamPakService.ResolveBdziamPakAsync(null, null, progress));
    }

    /// <summary>
    /// Resolves a BdziamPak package with empty metadata.
    /// </summary>
    [Fact]
    public async Task ResolveBdziamPakAsync_WithEmptyMetadata_ReturnsFalse()
    {
        var bdziamPakService = _serviceProvider.GetRequiredService<BdziamPakService>();

        var progress = new Progress<BdziamPakResolveProgress>();
        progress.ProgressChanged += (p, e) => outputHelper.WriteLine("{0}, Progress: {1}", e.Percent, e.Message);

        var result=
            await bdziamPakService.ResolveBdziamPakAsync(string.Empty, string.Empty, progress);
        
        Assert.False(result.Success);

    }
}