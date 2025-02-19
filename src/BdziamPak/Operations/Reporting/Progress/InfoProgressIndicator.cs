namespace BdziamPak.Operations.Reporting.Progress;

public class InfoProgressIndicator(string name) : ProgressIndicator(name)
{
    public string Info { get; private set; } = string.Empty;

    public void Update(string info)
    {
        Info = info;
    }
}