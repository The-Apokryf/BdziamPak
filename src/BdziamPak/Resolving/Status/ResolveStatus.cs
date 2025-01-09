namespace BdziamPak.Resolving;

public class ResolveStatus
{
    public List<ResolveStatusLog> Logs { get; } = new();
    public int CurrentStep => Logs.MaxBy(x => x.ResolveStep)?.ResolveStep ?? -1;
    public ResolveStatusLog? CurrentStatus => Logs.LastOrDefault();
    public event Action<ResolveStatusLog>? ResolveStatusChanged;

    public void AddStatus(int step, string message, int? percent = null)
    {
        Logs.Add(new ResolveStatusLog(step, message, percent));
        ResolveStatusChanged?.Invoke(CurrentStatus);
    }
}