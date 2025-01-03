using System.Text.Json.Serialization;

namespace BdziamPak.Packages.Packaging.Model;

public class BdziamPakMetadata
{
    /// <summary>
    /// Name of the BdziamPak
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Description of the BdziamPak
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Author of the BdziamPak
    /// </summary>
    public string Author { get; set; }
    /// <summary>
    /// Returns the BdziamPakId, which consists of Author and Name separated by a dot.
    /// </summary>
    public string BdziamPakId => $"{Author}.{Name}";
    
    /// <summary>
    /// Version of the BdziamPak
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// Repository Url of the BdziamPak
    /// </summary>
    public string? RepositoryUrl { get; set; }
    
    /// <summary>
    /// Other BdziamPaks that this BdziamPak depends on
    /// </summary>
    public List<BdziamPakDependency> BdziamPakDependencies { get; set; } = new();
    
    /// <summary>
    /// NuGet package for this BdziamPak
    /// </summary>
    public BdziamPakNuGetDependency NuGetPackage { get; set; } = new();
    
    /// <summary>
    /// Additional metadata for the BdziamPak
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> MetaData { get; set; } = new();
}