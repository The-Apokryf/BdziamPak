namespace BdziamPak.Operations.Reporting.Progress;

public class DeterminateProgressIndicator(string name) : ProgressIndicator(name)
{
    public int Current { get; protected set; }
    public int Total { get; protected set; }
    public int Percentage => Current / Math.Max(Total, 1) * 100;

    public DeterminateProgressIndicator UpdateProgress(int current, int total)
    {
        Total = total;
        Current = current;
        FinishIfCompleted();
        return this;
    }

    public DeterminateProgressIndicator UpdateProgress(int current)
    {
        Current = current;
        FinishIfCompleted();
        return this;
    }

    private void FinishIfCompleted()
    {
        if (Total > 0 && Current >= Total) Finish();
    }
}