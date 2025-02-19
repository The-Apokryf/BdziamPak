using System.Text.Json;
using BdziamPak.Directory;
using BdziamPak.Operations.Execution;
using BdziamPak.Operations.Factory;
using BdziamPak.PackageModel;
using BdziamPak.Sources.Model;
using BdziamPak.Tests;
using Microsoft.Extensions.DependencyInjection;

var badMetadata =
    JsonSerializer.SerializeToElement(TestData.BadMetadata).Deserialize<BdziamPakMetadata>();

var goodMetadata =
    JsonSerializer.SerializeToElement(TestData.GoodMetadata).Deserialize<BdziamPakMetadata>();

IServiceProvider serviceProvider;

serviceProvider = TestServiceProvider.CreateServiceProvider(
    config => { config.BdziamPakPath = ".testbdziampak"; },
    services => services);

var tempIndexPath = Path.Combine(
    serviceProvider.GetRequiredService<BdziamPakDirectory>().SourcesDirectory.FullName,
    "TestSource.json");

if (!File.Exists(tempIndexPath))
    File.WriteAllText(tempIndexPath, JsonSerializer.Serialize(new BdziamPakSourceIndex
    {
        Paks = [goodMetadata],
        Name = "TestSource",
        Description = "Test source for BdziamPak"
    }));
var progress = new Progress<BdziamPakOperationProgress>();
var initPosition = Console.GetCursorPosition();
progress.ProgressChanged += (sender, operationProgress) =>
{
    Console.SetCursorPosition(initPosition.Left, initPosition.Top);
    Redraw(operationProgress);

};
var operationFactory = serviceProvider.GetRequiredService<IOperationFactory>();
var operation = operationFactory.GetOperation("resolve");
var executor = serviceProvider.GetRequiredService<BdziamPakOperationExecutor>();
try
{
    
    await executor.ExecuteOperation("TestAuthor.testPak@1.0.0", operation, progress);
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Console.ReadKey();
}

Console.ReadKey();


void Clear()
{
    
}

void Redraw(BdziamPakOperationProgress progress)
{
    
}