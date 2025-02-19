namespace BdziamPak.Operations.Reporting.Progress;

public abstract class ProgressIndicator(string name)
{
    public string Name => name;
    public bool IsFinished { get; set; }
    public bool IsError { get; set; }

    public void Finish(bool error = false)
    {
        IsFinished = true;
        IsError = error;
    }
}