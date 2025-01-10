namespace BdziamPak.Resolving;

/// <summary>
/// Represents the status of the resolving process.
/// </summary>
public class ResolveStatus
{
    /// <summary>
    /// Gets the list of resolve status logs.
    /// </summary>
    public List<ResolveStatusLog> Logs { get; } = new();

    /// <summary>
    /// Gets the current step of the resolving process.
    /// </summary>
    public int CurrentStep => Logs.MaxBy(x => x.ResolveStep)?.ResolveStep ?? -1;

    /// <summary>
    /// Gets the current status log of the resolving process.
    /// </summary>
    public ResolveStatusLog? CurrentStatus => Logs.LastOrDefault();

    /// <summary>
    /// Occurs when the resolve status changes.
    /// </summary>
    public event Action<ResolveStatusLog>? ResolveStatusChanged;

    /// <summary>
    /// Adds a new status log to the resolving process.
    /// </summary>
    /// <param name="step">The step number of the resolving process.</param>
    /// <param name="message">The message describing the status.</param>
    /// <param name="percent">The optional percentage of completion.</param>
    public void AddStatus(int step, string message, int? percent = null)
    {
        Logs.Add(new ResolveStatusLog(step, message, percent));
        ResolveStatusChanged?.Invoke(CurrentStatus);
    }
}