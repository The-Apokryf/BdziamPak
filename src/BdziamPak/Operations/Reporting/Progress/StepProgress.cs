using BdziamPak.Operations.Reporting.States;

namespace BdziamPak.Operations.Reporting.Progress;

public class StepProgress
{
    private readonly IProgress<StepProgress> _progress;

    public StepProgress(string stepName, IProgress<StepProgress> progress)
    {
        StepName = stepName;
        _progress = progress;
        ProgressIndicators = new ProgressIndicators(this);
    }

    public string StepName { get; }

    public int Percentage => ProgressIndicators.PercentageCompleted;

    public ProgressIndicators ProgressIndicators { get; }

    public StepState StepState { get; private set; }

    private readonly object _lockObject = new();

    public void UpdateAndReport(string message, StepState? state = null)
    {
        Update(message, state);
        Report();
    }

    public StepProgress Update(string message, StepState? state = null)
    {
        if (state != null)
        {
            StepState = state.Value;
            if(StepState == StepState.Failed) Finish(message, true);
        }
        this.Status(message);
        return this;
    }

    public void Finish(string message, bool isError = false)
    {
        this.Status(message);
        StepState = isError ? StepState.Failed : StepState.Success;

        foreach (var progressIndicator in ProgressIndicators.Progress)
        {
            progressIndicator.Finish(isError);
        }

        Report();
    }
    
    
    public void FinishIndicator(string name, bool isError = false)
    {
        StepState = isError ? StepState.Failed : StepState.Success;
        ProgressIndicators.Finish(name, isError);
        Report();
    }

    
    public void Skip(string message)
    {
        StepState = StepState.Skipped;

        foreach (var progressIndicator in ProgressIndicators.Progress)
        {
            progressIndicator.Finish();
        }

        this.Status(message);
        Report();
    }

    public StepProgress Update<T>(string name, Action<T> updateAction) where T : ProgressIndicator
    {
        ProgressIndicators.UpdateProgress<T>(name, updateAction);
        return this;
    }

    public void Report()
    {
        lock (_lockObject)
        {
            _progress.Report(this);
        }
    }
}