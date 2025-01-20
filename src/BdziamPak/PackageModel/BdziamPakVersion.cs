using System.Text.Json;
using System.Text.Json.Serialization;

namespace BdziamPak.PackageModel;

/// <summary>
///     Versions of given BdziamPak
/// </summary>
public class BdziamPakVersion
{
    /// <summary>
    ///     Gets or sets the version of the BdziamPak.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     Gets or sets additional metadata for the BdziamPak.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Metadata { get; set; } = new();

    /// <summary>
    ///     Gets the metadata value for the specified key.
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
    ///     Determines whether the metadata contains the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the metadata contains the key; otherwise, false.</returns>
    public bool HasMetadata(string key)
    {
        return Metadata.ContainsKey(key);
    }
}