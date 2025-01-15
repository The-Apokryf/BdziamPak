using Bdziam.ExternalDependencyResolver;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps;

namespace BdziamPak.Operations;

/// <summary>
/// Represents the Operation on BdziamPak package.
/// </summary>
public class BdziamPakOperation(
    string operationName,
    ExternalDependencyResolver externalDependencyResolver)
{
    /// <summary>
    /// List of steps involved in the process.
    /// </summary>
    protected readonly List<BdziamPakOperationStep> _steps = new();

    public IReadOnlyList<BdziamPakOperationStep> Steps => _steps;
    
    public IReadOnlyList<BdziamPakOperationStep> CompletedSteps => _steps
        .Where(x => x.StepState is StepState.Success or StepState.Skipped)
        .ToList();

    /// <summary>
    /// Name of the operation
    /// </summary>
    public string OperationName => operationName;
    
    /// <summary>
    /// Adds a step to the resolving process.
    /// </summary>
    /// <typeparam name="TStep">The type of the step to add.</typeparam>
    /// <returns>The current instance of <see cref="BdziamPakOperation"/>.</returns>
    public BdziamPakOperation AddStep<TStep>() where TStep : BdziamPakOperationStep
    {
        var step = externalDependencyResolver.Resolve<TStep>();
        _steps.Add(step);
        return this;
    }
}