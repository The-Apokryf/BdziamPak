using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Packaging.Install.Model;

namespace BdziamPak.Resolving.ResolveSteps.BuiltIn;

public class ResolveBdziamPakDependenciesStep(BdziamPakService bdziamPakService) : BdziamPakResolveStep
{
    public override string StepName => "ResolveBdziamPakDependencies";
    public override string StepDescription => "Resolves BdziamPak dependencies for the package.";

    public override bool CanExecute(ICheckResolveContext context)
    {
        return context.HasMetadata("Dependencies");
    }

    public override async Task ExecuteAsync(IExecutionResolveContext context)
    {
        context.UpdateStatus("Resolving BdziamPak dependencies...");

        var dependencies = context.BdziamPakMetadata.GetMetadata<List<BdziamPakDependency>>("Dependencies");
        foreach (var dependency in dependencies)
        {
            var progress = new Progress<BdziamPakInstallProgress>();
            progress.ProgressChanged += (p, e) => { context.UpdateStatus(e.Message, (int)e.ProgressPercentage); };
            await bdziamPakService.ResolveBdziamPakAsync(dependency.BdziamPakId, dependency.Version, progress);
        }

        context.Complete();
    }
}