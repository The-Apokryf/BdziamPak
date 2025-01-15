namespace BdziamPak.PackageModel.Builtin;

/// <summary>
///     Reference to a git repository that needs to be downloaded with BdziamPak
/// </summary>
public class BdziamPakRepositoryReference
{
    /// <summary>
    ///     Url of the repository
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Hash of the commit that should be downloaded
    /// </summary>
    public string CommitHash { get; set; }
}