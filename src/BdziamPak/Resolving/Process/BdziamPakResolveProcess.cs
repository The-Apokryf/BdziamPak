using Bdziam.ExternalDependencyResolver;
using BdziamPak.Packages.Packaging.Model;
using BdziamPak.Resolving.ResolveSteps;
using BdziamPak.Structure;
using Microsoft.Extensions.Logging;

namespace BdziamPak.Resolving;

/// <summary>
///     Package Resolvement is split into steps,
/// </summary>
public class BdziamPakResolveProcess(
    BdziamPakDirectory directory,
    ILogger<BdziamPakResolveProcess> logger,
    ExternalDependencyResolver externalDependencyResolver)
{
    protected readonly List<BdziamPakResolveStep> _steps = new();
    protected readonly string Id = Guid.NewGuid().ToString();
    public BdziamPakResolveStep? CurrentStep { get; protected set; }
    public IProgress<ResolveStatusLog>? Progress { get; set; }

    public void StepStopped(bool fatal, BdziamPakResolveContext context)
    {
        logger.LogWarning("{fatal}Resolve stopped with state {State}", fatal ? "[FATAL] " : "", context.State);
    }

    public void StepResolveCompleted(BdziamPakResolveContext context)
    {
        Progress?.Report(context.Status.CurrentStatus);
    }

    public void UpdateProcess(BdziamPakResolveContext context)
    {
        Progress?.Report(context.Status.CurrentStatus ?? new ResolveStatusLog(-1, "Waiting..."));
    }

    public BdziamPakResolveProcess AddStep<TStep>() where TStep : BdziamPakResolveStep
    {
        var step = externalDependencyResolver.Resolve<TStep>();
        _steps.Add(step);
        return this;
    }

    public async Task Execute(BdziamPakMetadata metadata)
    {
        var context = new BdziamPakResolveContext(this, directory);
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