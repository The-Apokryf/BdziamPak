using System.Globalization;
using BdziamPak.Operations;
using BdziamPak.Operations.Execution;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Spectre.StepRendering.Factory;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BdziamPak.Spectre;

public class OperationRenderer
{
    public void Render(string operationText, Progress<BdziamPakOperationProgress> operationProgress, IAnsiConsole? console = null)
    {
        if (console == null)
        {
            console = AnsiConsole.Console;
        }
        
        console.MarkupLine("Executing operation: [bold]{0}[/]", Markup.Escape(operationText));
        console.Progress()
            .AutoRefresh(false) // Turn off auto refresh
            .AutoClear(false)   // Do not remove the task list when done
            .HideCompleted(false)   // Hide tasks as they are completed
            .Columns(new ProgressColumn[] 
            {
                new TaskDescriptionColumn(),    // Task description
                new ProgressBarColumn(),        // Progress bar
                new PercentageColumn(),         // Percentage
                new SpinnerColumn(),            // Spinner
            })
            .Start(ctx =>
            {
                operationProgress.ProgressChanged += (sender, progress) =>
                {
                    var currentStep = GetCurrentStep(progress);
                    var task = ctx.AddTask(progress.Message);
                    task.Description = currentStep == null ? progress.Message : $"({progress.Steps.IndexOf(currentStep)+1}/{progress.Steps.Count}) {progress.Message}";
                    task.MaxValue = 100;
                    task.Value = progress.Progress;
                };
            });
        
    }

    private BdziamPakStepProgress? GetCurrentStep(BdziamPakOperationProgress operationProgress)
    {
        return operationProgress.Steps.FirstOrDefault(s => s.State == StepState.Running);
    }
    
}