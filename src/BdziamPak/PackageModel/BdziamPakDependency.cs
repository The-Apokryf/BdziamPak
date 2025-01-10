namespace BdziamPak.Packages.Packaging.Model;

/// <summary>
/// Represents a dependency in the BdziamPak package.
/// </summary>
public class BdziamPakDependency
{
    /// <summary>
    /// Gets or sets the ID of the BdziamPak package.
    /// </summary>
    public string BdziamPakId { get; set; }

    /// <summary>
    /// Gets or sets the version of the BdziamPak package.
    /// </summary>
    public string Version { get; set; }
}