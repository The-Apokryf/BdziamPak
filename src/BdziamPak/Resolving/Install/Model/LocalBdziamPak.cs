namespace BdziamPak.Packaging.Install.Model;

/// <summary>
/// Represents a local BdziamPak package with its dependencies.
/// </summary>
public class LocalBdziamPak
{
    /// <summary>
    /// Gets or sets the ID of the BdziamPak package.
    /// </summary>
    public string BdziamPakId { get; set; }

    /// <summary>
    /// Gets or sets the version of the BdziamPak package.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the dependencies of the BdziamPak package.
    /// </summary>
    public LocalBdziamPak[] Dependencies { get; set; }
}