using BdziamPak.Operations.Reporting.Progress;
using BdziamPak.Operations.Reporting.States;
using BdziamPak.Operations.Steps;

namespace BdziamPak.Operations.Execution;

public class BdziamPakOperationProgress
{
    private readonly BdziamPakOperation _operation;
    private readonly IProgress<BdziamPakOperationProgress> _progress;
    private readonly object _lockObject = new();


    public BdziamPakOperationProgress(IProgress<BdziamPakOperationProgress> progress, BdziamPakOperation operation)
    {
        _progress = progress;
        _operation = operation;
        CurrentOperationState = OperationState.Started;
        Steps = operation.Steps.Select(s =>
        {
            var stepProgress = new Progress<StepProgress>();
            stepProgress.ProgressChanged += (sender, e) =>
            {
                lock (_lockObject)
                {
                    if (e.StepState == StepState.Failed)
                        Update($"Operation Failed, due to step {e.StepName} failing.", OperationState.Failed);
                    _progress.Report(this);
                }
            };
            return (progress: stepProgress, model: new StepProgress(s.StepName, stepProgress));
        }).ToDictionary(k => k.model, v => v.progress);
    }

    public OperationState CurrentOperationState { get; private set; }
    public IReadOnlyDictionary<StepProgress, Progress<StepProgress>> Steps { get; }
    public int Progress => Steps.Keys.Sum(x => x.Percentage) / (Steps.Count * 100);
    public string Message { get; private set; }

    public void Update(string message, OperationState? state = null)
    {
        Message = message;
        if (state != null) CurrentOperationState = state.Value;
        _progress.Report(this);
    }

    public StepProgress? GetStepProgress(BdziamPakOperationStep step)
    {
        return Steps.Keys.FirstOrDefault(a => a.StepName == step.StepName);
    }
}