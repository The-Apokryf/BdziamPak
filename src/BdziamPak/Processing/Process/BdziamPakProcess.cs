using Bdziam.ExternalDependencyResolver;
using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Resolving.ResolveSteps;
using BdziamPak.Structure;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Resolving;

/// <summary>
/// Represents the process of resolving a BdziamPak package.
/// </summary>
public class BdziamPakProcess(
    BdziamPakDirectory directory,
    ILogger<BdziamPakProcess> logger,
    ExternalDependencyResolver externalDependencyResolver)
{
    /// <summary>
    /// List of steps involved in the resolving process.
    /// </summary>
    protected readonly List<BdziamPakProcessStep> _steps = new();

    /// <summary>
    /// Unique identifier for the resolve process.
    /// </summary>
    protected readonly string Id = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the current step in the resolving process.
    /// </summary>
    public BdziamPakProcessStep? CurrentStep { get; protected set; }

    /// <summary>
    /// Gets or sets the progress reporter for the resolving process.
    /// </summary>
    public IProgress<ResolveStatusLog>? Progress { get; set; }

    /// <summary>
    /// Logs a warning when a step in the resolving process is stopped.
    /// </summary>
    /// <param name="fatal">Indicates if the stop is fatal.</param>
    /// <param name="context">The context of the resolving process.</param>
    public void StepStopped(bool fatal, BdziamPakProcessingContext context)
    {
        logger.LogWarning("{fatal}Process stopped with state {State}", fatal ? "[FATAL] " : "", context.State);
    }

    /// <summary>
    /// Reports the completion of a step in the resolving process.
    /// </summary>
    /// <param name="context">The context of the resolving process.</param>
    public void StepResolveCompleted(BdziamPakProcessingContext context)
    {
        Progress?.Report(context.Status.CurrentStatus);
    }

    /// <summary>
    /// Updates the progress of the resolving process.
    /// </summary>
    /// <param name="context">The context of the resolving process.</param>
    public void UpdateProcess(BdziamPakProcessingContext context)
    {
        Progress?.Report(context.Status.CurrentStatus ?? new ResolveStatusLog(-1, "Waiting..."));
    }

    /// <summary>
    /// Adds a step to the resolving process.
    /// </summary>
    /// <typeparam name="TStep">The type of the step to add.</typeparam>
    /// <returns>The current instance of <see cref="BdziamPakProcess"/>.</returns>
    public BdziamPakProcess AddStep<TStep>() where TStep : BdziamPakProcessStep
    {
        var step = externalDependencyResolver.Resolve<TStep>();
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executes the resolving process with the given metadata.
    /// </summary>
    /// <param name="metadata">The metadata of the BdziamPak package.</param>
    public async Task Execute(BdziamPakMetadata metadata)
    {
        var context = new BdziamPakProcessingContext(this, directory);
        context.BdziamPakMetadata = metadata;
        if (!context.ResolveDirectory.Exists)
            context.ResolveDirectory.Create();
        logger.LogInformation("Executing Process {Id}", Id);
        foreach (var step in _steps)
        {
            if (context.State == ResolveState.Failed || context.State == ResolveState.Aborted)
                StepStopped(context.State == ResolveState.Failed, context);
            CurrentStep = step;
            logger.LogInformation("Checking if Step {StepName} can be executed", step.StepName);
            if (step.CanExecute(context))
            {
                logger.LogInformation("Executing Step {StepName}", step.StepName);
                await step.ExecuteAsync(context);
                logger.LogInformation("Executing Step {StepName} Complete, ", step.StepName);
            }
        }
    }
}