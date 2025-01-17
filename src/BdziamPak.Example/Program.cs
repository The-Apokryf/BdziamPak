using System.Reflection;
using System.Text.Json;
using BdziamPak.Directory;
using BdziamPak.Operations.Execution;
using BdziamPak.Operations.Factory;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.PackageModel;
using BdziamPak.Sources.Model;
using BdziamPak.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

BdziamPakMetadata badMetadata =
    JsonSerializer.SerializeToElement(TestData.BadMetadata).Deserialize<BdziamPakMetadata>();

BdziamPakMetadata goodMetadata =
    JsonSerializer.SerializeToElement(TestData.GoodMetadata).Deserialize<BdziamPakMetadata>();

IServiceProvider serviceProvider;

serviceProvider = TestServiceProvider.CreateServiceProvider(
    config => { config.BdziamPakPath = ".testbdziampak"; },
    services => services.AddLogging(log => log.AddConsole()));

var tempIndexPath = Path.Combine(
    serviceProvider.GetRequiredService<BdziamPakDirectory>().SourcesDirectory.FullName,
    "TestSource.json");

if (!File.Exists(tempIndexPath))
    File.WriteAllText(tempIndexPath, JsonSerializer.Serialize(new BdziamPakSourceIndex()
    {
        Paks = [goodMetadata],
        Name = "TestSource",
        Description = "Test source for BdziamPak"
    }));

var operationFactory = serviceProvider.GetRequiredService<IOperationFactory>();
        
var operation = operationFactory.GetOperation("resolve");
var executor = serviceProvider.GetRequiredService<BdziamPakOperationExecutor>();
var progress = new Progress<BdziamPakOperationProgress>();
AnsiConsole.Status()
    .AutoRefresh(false)
    .Spinner(Spinner.Known.Star)
    .SpinnerStyle(Style.Parse("green bold"))
    .Start("Thinking...", ctx => 
    {
        // Update task progress based on BdziamPakOperationProgress
        progress.ProgressChanged += (p, e) =>
        {
            ctx.Status = e.Message;
            ctx.Refresh();
        };
    });
AnsiConsole.Progress()
    .AutoRefresh(false) // Turn on auto refresh
    .AutoClear(false)  // Do not remove the task list when done
    .Columns(new ProgressColumn[]
    {
        new TaskDescriptionColumn(),    // Task description
        new ProgressBarColumn(),        // Progress bar
        new PercentageColumn(),         // Percentage
        new RemainingTimeColumn(),      // Remaining time
        new SpinnerColumn(), // Spinner
    })
    .Start(ctx =>
    {
        // Define tasks for each step
        var tasks = new Dictionary<string, ProgressTask>();
        foreach (var step in operation.Steps)
        {
            tasks[step.StepName] = ctx.AddTask($"[green]{step.StepName}[/]", new ProgressTaskSettings()
            {
                MaxValue = 100,
            });
        }
        
        // Update task progress based on BdziamPakOperationProgress
        progress.ProgressChanged += (p, e) =>
        {
            foreach (var stepProgress in e.Steps)
            {

                if (tasks.TryGetValue(stepProgress.Name, out var task))
                {
                    if (stepProgress.State == StepState.Running)
                    {
                       //task.State.Update(stepProgress.)
                    }
                    task.MaxValue = 100;
                    task.Value = stepProgress.Percentage;
                    if (stepProgress.StepProgressModel is { } stepProgressModel)
                    {
                        foreach (var propertyInfo in stepProgressModel.GetType().GetProperties())
                        {
                            //task.State.Update<string>(propertyInfo.Name,  o => propertyInfo.GetValue(o).ToString());

                        }
                    }
                    task.Description = "[green]" + Markup.Escape($"[{stepProgress.State}] {stepProgress.Name} ({stepProgress.Message})") + "[/]";
                }
            }

            // Report overall operation progress
            ctx.Refresh();
        };
       
    });
        
//var state = await executor.ExecuteOperation("TestAuthor.testPak@1.0.0", operation, progress);
