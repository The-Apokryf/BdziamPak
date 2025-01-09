namespace BdziamPak.Resolving.ResolveSteps;

public abstract class BdziamPakResolveStep
{
    public abstract string StepName { get; }
    public abstract string StepDescription { get; }
    public abstract bool CanExecute(ICheckResolveContext context);
    public abstract Task ExecuteAsync(IExecutionResolveContext context);
}