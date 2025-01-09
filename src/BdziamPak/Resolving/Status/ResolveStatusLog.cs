namespace BdziamPak.Resolving;

public class ResolveStatusLog(int step, string message, int? percent = null)
{
    public int ResolveStep => step;
    public string Message => message;
    public int? Percent => percent;

    public override string ToString()
    {
        return $"ResolveStep: {ResolveStep}, Message: {Message} {(Percent != null ? $"{Percent}%" : "")}";
    }
}