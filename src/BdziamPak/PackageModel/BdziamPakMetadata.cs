using System.Text.Json;
using System.Text.Json.Serialization;

namespace BdziamPak.Packages.Packaging.Model;

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

    /// <summary>
    /// Gets or sets the version of the BdziamPak.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the BdziamPak.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();

    /// <summary>
    /// Gets the metadata value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="key">The key of the metadata.</param>
    /// <returns>The metadata value if found; otherwise, the default value for the type.</returns>
    public T? GetMetadata<T>(string key)
    {
        if (Metadata.TryGetValue(key, out var value))
            return value.Deserialize<T>();

        return default;
    }

    /// <summary>
    /// Determines whether the metadata contains the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the metadata contains the key; otherwise, false.</returns>
    public bool HasMetadata(string key)
    {
        return Metadata.ContainsKey(key);
    }
}