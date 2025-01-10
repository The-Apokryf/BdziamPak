namespace BdziamPak.Resolving.ResolveSteps;

/// <summary>
/// Abstract base class representing a step in the BdziamPak resolving process.
/// </summary>
public abstract class BdziamPakResolveStep
{
    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    public abstract string StepName { get; }

    /// <summary>
    /// Gets the description of the step.
    /// </summary>
    public abstract string StepDescription { get; }

    /// <summary>
    /// Determines whether the step can be executed based on the provided context.
    /// </summary>
    /// <param name="context">The context to check for execution eligibility.</param>
    /// <returns><c>true</c> if the step can be executed; otherwise, <c>false</c>.</returns>
    public abstract bool CanExecute(ICheckResolveContext context);

    /// <summary>
    /// Executes the step asynchronously.
    /// </summary>
    /// <param name="context">The context for the execution of the step.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public abstract Task ExecuteAsync(IExecutionResolveContext context);
}