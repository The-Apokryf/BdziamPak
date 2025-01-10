namespace BdziamPak.Resolving;

/// <summary>
/// Represents a log entry for the resolve status.
/// </summary>
/// <param name="step">The step number of the resolving process.</param>
/// <param name="message">The message describing the status.</param>
/// <param name="percent">The optional percentage of completion.</param>
public class ResolveStatusLog(int step, string message, int? percent = null)
{
    /// <summary>
    /// Gets the step number of the resolving process.
    /// </summary>
    public int ResolveStep => step;

    /// <summary>
    /// Gets the message describing the status.
    /// </summary>
    public string Message => message;

    /// <summary>
    /// Gets the optional percentage of completion.
    /// </summary>
    public int? Percent => percent;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"ResolveStep: {ResolveStep}, Message: {Message} {(Percent != null ? $"{Percent}%" : "")}";
    }
}