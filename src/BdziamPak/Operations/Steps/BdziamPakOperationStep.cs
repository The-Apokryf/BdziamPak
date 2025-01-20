using BdziamPak.Operations.Context;
using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps.Validation;

namespace BdziamPak.Operations.Steps;

/// <summary>
///     Abstract base class representing a step in the BdziamPak operation.
/// </summary>
public abstract class BdziamPakOperationStep
{
    /// <summary>
    ///     Gets the name of the step.
    /// </summary>
    public abstract string StepName { get; }

    /// <summary>
    ///     Gets or sets the current state of the step.
    /// </summary>
    public StepState StepState { get; set; }


    /// <summary>
    ///     Validates the operation in the given context.
    /// </summary>
    /// <param name="context">The context for validating the operation.</param>
    public abstract void ValidateOperation(OperationValidationContext context);

    /// <summary>
    ///     Executes the step asynchronously.
    /// </summary>
    /// <param name="context">The context for the execution of the step.</param>
    /// <param name="progress">The progress reporter for the step.</param>
    /// <param name="cancellationToken">Cancellation token passed to cancel the execution of the step.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public abstract Task ExecuteAsync(IExecuteOperationContext context, StepProgress progress,
        CancellationToken cancellationToken = default);
}