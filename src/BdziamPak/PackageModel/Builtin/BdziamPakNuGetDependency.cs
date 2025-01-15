namespace BdziamPak.PackageModel.Builtin;

/// <summary>
/// Represents a NuGet dependency in the BdziamPak package.
/// </summary>
public class BdziamPakNuGetDependency
{
    /// <summary>
    /// Gets or sets the ID of the NuGet package.
    /// </summary>
    public string PackageId { get; set; }

    /// <summary>
    /// Gets or sets the version of the NuGet package.
    /// </summary>
    public string PackageVersion { get; set; }

    /// <summary>
    /// Gets or sets the URL of the NuGet feed.
    /// </summary>
    public string? NuGetFeedUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include prerelease versions.
    /// </summary>
    public bool Prerelease { get; set; } = false;
}