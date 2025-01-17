using BdziamPak.NuGetPackages.Download.Model;
using Spectre.Console;

namespace BdziamPak.Spectre.StepRendering.BuiltIn;

/// <summary>
/// Example renderer for NuGetDownloadProgress models.
/// </summary>
public class NuGetDownloadProgressRenderer : StepProgressRendererBase<NuGetDownloadProgress>
{
    public override void Render(NuGetDownloadProgress model, IAnsiConsole console)
    {
        var message = model.Message ?? "Downloading...";
        var percent = model.Percent.HasValue ? model.Percent.Value : 0;
        console.WriteLine($"[bold]NuGet Download Progress[/]: {message} ({percent}%)");
    }
}