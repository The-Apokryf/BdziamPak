using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Packaging.Install.Model;

namespace BdziamPak.Resolving.ResolveSteps.BuiltIn;

/// <summary>
/// Represents a step in the resolving process that resolves BdziamPak dependencies for the package.
/// </summary>
/// <param name="bdziamPakService">The service used to resolve BdziamPak dependencies.</param>
public class ResolveBdziamPakDependenciesStep(BdziamPakService bdziamPakService) : BdziamPakResolveStep
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    public override string StepName => "ResolveBdziamPakDependencies";

    /// <summary>
    /// Gets the description of the step.
    /// </summary>
    public override string StepDescription => "Resolves BdziamPak dependencies for the package.";

    /// <summary>
    /// Determines whether the step can be executed based on the provided context.
    /// </summary>
    /// <param name="context">The context to check for execution eligibility.</param>
    /// <returns><c>true</c> if the step can be executed; otherwise, <c>false</c>.</returns>
    public override bool CanExecute(ICheckResolveContext context)
    {
        return context.HasMetadata("Dependencies");
    }

    /// <summary>
    /// Executes the step asynchronously.
    /// </summary>
    /// <param name="context">The context for the execution of the step.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExecuteAsync(IExecutionResolveContext context)
    {
        context.UpdateStatus("Resolving BdziamPak dependencies...");

        var dependencies = context.BdziamPakMetadata.GetMetadata<List<BdziamPakDependency>>("Dependencies");
        foreach (var dependency in dependencies)
        {
            var progress = new Progress<BdziamPakResolveProgress>();
            progress.ProgressChanged += (p, e) => { context.UpdateStatus(e.Message, (int)e.Percent!); };
            await bdziamPakService.ResolveBdziamPakAsync(dependency.BdziamPakId, dependency.Version, progress);
        }

        context.Complete();
    }
}