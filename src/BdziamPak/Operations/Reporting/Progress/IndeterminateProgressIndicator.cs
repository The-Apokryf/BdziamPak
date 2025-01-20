namespace BdziamPak.Operations.Reporting.Progress;

public class IndeterminateProgressIndicator(string name) : ProgressIndicator(name)
{
    public string? Status { get; set; }

    public IndeterminateProgressIndicator Update(string status)
    {
        Status = status;
        return this;
    }
}