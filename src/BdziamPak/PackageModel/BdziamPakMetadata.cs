namespace BdziamPak.PackageModel;

/// <summary>
/// Represents metadata for a BdziamPak package.
/// </summary>
public class BdziamPakMetadata
{
    /// <summary>
    /// Gets or sets the name of the BdziamPak.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the BdziamPak.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the author of the BdziamPak.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets the BdziamPakId, which consists of Author and Name separated by a dot.
    /// </summary>
    public string BdziamPakId => $"{Author}.{Name}";

    public List<BdziamPakVersion> Versions { get; set; } = new();
    
    public bool VersionExists(string version) => Versions.Any(v => v.Version == version);
    
    public BdziamPakVersion? this[string version] => Versions.FirstOrDefault(v => v.Version == version);
}