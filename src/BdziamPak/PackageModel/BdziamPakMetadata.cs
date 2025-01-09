using System.Text.Json;
using System.Text.Json.Serialization;

namespace BdziamPak.Packages.Packaging.Model;

public class BdziamPakMetadata
{
    /// <summary>
    ///     Name of the BdziamPak
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Description of the BdziamPak
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Author of the BdziamPak
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    ///     Returns the BdziamPakId, which consists of Author and Name separated by a dot.
    /// </summary>
    public string BdziamPakId => $"{Author}.{Name}";

    /// <summary>
    ///     Version of the BdziamPak
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     Additional metadata for the BdziamPak
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();

    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value))
            return value.Deserialize<T>();

        return default;
    }

    public bool HasMetadata(string key)
    {
        return Metadata.ContainsKey(key);
    }
}