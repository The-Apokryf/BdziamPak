using BdziamPak.Resolving.ResolveSteps;

namespace BdziamPak.Processing.Process.Progress;

public class StepStatus(BdziamPakProcessStep step)
{
    public BdziamPakProcessStep Step { get; } = step;
    public event Action<
}