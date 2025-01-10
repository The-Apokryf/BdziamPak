using BdziamPak.Resolving;

namespace BdziamPak.Packaging.Install.Model;

/// <summary>
/// Represents the progress of resolving a BdziamPak package.
/// </summary>
public class BdziamPakResolveProgress
{
    /// <summary>
    /// Gets or sets the list of resolve status logs.
    /// </summary>
    public List<ResolveStatusLog> ResolveStatusLogs { get; set; } = new();

    /// <summary>
    /// Gets or sets the current progress message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the current progress percentage.
    /// </summary>
    public int? Percent { get; set; }
}