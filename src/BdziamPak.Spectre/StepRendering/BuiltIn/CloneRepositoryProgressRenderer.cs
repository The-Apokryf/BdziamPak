using BdziamPak.Git;
using Spectre.Console;

namespace BdziamPak.Spectre.StepRendering.BuiltIn;

/// <summary>
/// Example renderer for CloneRepositoryProgress models.
/// </summary>
public class CloneRepositoryProgressRenderer : StepProgressRendererBase<CloneRepositoryProgress>
{
    
    public override void Render(CloneRepositoryProgress model, IAnsiConsole console)
    {
        var message = model.Message ?? "Cloning repository...";
        var cloneProgress = model.CloneProgress;
        console.WriteLine($"[bold]Git Clone Progress[/]: {message} – Overall: {cloneProgress}% (Fetch: {model.FetchProgress}%, Checkout: {model.CheckoutProgress}%)");
        if (!string.IsNullOrEmpty(model.Path))
        {
            console.WriteLine($"Path: {model.Path}");
        }
    }
}