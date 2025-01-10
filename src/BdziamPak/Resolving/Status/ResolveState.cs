namespace BdziamPak.Resolving;

/// <summary>
/// Represents the state of the resolving process.
/// </summary>
public enum ResolveState
{
    /// <summary>
    /// The resolving process is in the initialization state.
    /// </summary>
    Initialization,

    /// <summary>
    /// The resolving process has started.
    /// </summary>
    Started,

    /// <summary>
    /// The resolving process is complete.
    /// </summary>
    Complete,

    /// <summary>
    /// The resolving process has failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The resolving process has been aborted.
    /// </summary>
    Aborted
}