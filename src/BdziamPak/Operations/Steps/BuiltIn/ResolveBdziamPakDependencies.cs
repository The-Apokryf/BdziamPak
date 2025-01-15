using BdziamPak.Operations.Context;
using BdziamPak.Operations.Execution;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps.Validation;
using BdziamPak.Operations.Steps.Validation.BuiltIn;
using BdziamPak.PackageModel;
using StepProgress = BdziamPak.Operations.Reporting.Progress.StepProgress;

namespace BdziamPak.Operations.Steps.BuiltIn;

/// <summary>
/// Represents a step in the resolving process that resolves BdziamPak dependencies for the package.
/// </summary>
/// <param name="bdziamPakService">The service used to resolve BdziamPak dependencies.</param>
public class ResolveBdziamPakDependencies(BdziamPakOperationExecutor executor) : BdziamPakOperationStep
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    public override string StepName => "ResolveBdziamPakDependencies";
    private const string DependenciesMetadataKey = "Dependencies";
    public override void ValidateOperation(OperationValidationContext context)
    {
        context.AddCondition<HasMetadataCondition>(condition => condition.RequireMetadata(DependenciesMetadataKey));
    }

    public override async Task ExecuteAsync(IExecuteOperationContext context, IProgress<StepProgress> progress, CancellationToken cancellationToken = default)
    {
        progress.Report(("Resolving BdziamPak dependencies...", 0, "Resolving BdziamPak dependencies..."));

        var dependencies = context.GetMetadata<List<BdziamPakDependency>>(DependenciesMetadataKey);
        foreach (var dependency in dependencies)
        {
            
        }
        
        
        
        StepState = StepState.Success;
    }
}