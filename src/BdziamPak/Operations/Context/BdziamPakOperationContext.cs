using BdziamPak.Operations.Steps;
using BdziamPak.PackageModel;

namespace BdziamPak.Operations.Context;

/// <summary>
///     Represents the context for resolving a BdziamPak package.
/// </summary>
/// <param name="bdziamPakOperation">The resolve process.</param>
/// <param name="directory">The directory where the package is located.</param>
public class BdziamPakOperationContext(BdziamPakOperation bdziamPakOperation)
    : IExecuteOperationContext, IValidationContext
{
    private readonly Dictionary<string, object> _contextParameters = new();

    /// <summary>
    ///     Gets or sets the Version for the requested BdziamPak package.
    /// </summary>
    public string RequestedVersion { get; set; }

    public T? GetContextParameter<T>(string name)
    {
        if (!_contextParameters.TryGetValue(name, out var value)) return default;

        if (value is T typedValue) return typedValue;

        return default;
    }

    public void SetContextParameter<T>(string name, T value)
    {
        if (!_contextParameters.TryAdd(name, value!)) _contextParameters[name] = value!;
    }

    /// <summary>
    ///     Gets the resolve directory.
    /// </summary>
    public DirectoryInfo ResolveDirectory { get; set; }

    /// <summary>
    ///     Gets or sets the metadata for the BdziamPak package.
    /// </summary>
    public BdziamPakMetadata BdziamPakMetadata { get; set; }

    /// <summary>
    ///     Gets the metadata value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="key">The key of the metadata.</param>
    /// <returns>The metadata value if found; otherwise, the default value for the type.</returns>
    public T? GetMetadata<T>(string key)
    {
        var result = BdziamPakMetadata[RequestedVersion]!.GetMetadata<T>(key);
        return result;
    }

    /// <summary>
    ///     Checks if a specific resolve step was completed.
    /// </summary>
    /// <typeparam name="TStep">The type of the resolve step.</typeparam>
    /// <returns>true if the step was completed; otherwise, false.</returns>
    public bool WasCompleted<TStep>() where TStep : BdziamPakOperationStep
    {
        var result = bdziamPakOperation.CompletedSteps.FirstOrDefault(step => step.GetType() == typeof(TStep));
        if (result == null) return false;

        return true;
    }

    /// <summary>
    ///     Checks if a file exists in the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
    public bool FileExists(string relativePath)
    {
        var result = new FileInfo(Path.Combine(ResolveDirectory.FullName, relativePath)).Exists;
        if (!result) return false;

        return result;
    }

    /// <summary>
    ///     Checks if a directory exists in the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    /// <returns>true if the directory exists; otherwise, false.</returns>
    public bool DirectoryExists(string relativePath)
    {
        var result = new DirectoryInfo(Path.Combine(ResolveDirectory.FullName, relativePath)).Exists;
        if (!result) return false;

        return result;
    }

    /// <summary>
    ///     Checks if the metadata contains a specific key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the metadata contains the key; otherwise, false.</returns>
    public bool HasMetadata(string key)
    {
        var result = BdziamPakMetadata[RequestedVersion]!.HasMetadata(key);
        if (!result) return false;

        return result;
    }

    /// <summary>
    ///     Gets a file from the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>The file information.</returns>
    public FileInfo GetFile(string relativePath)
    {
        return new FileInfo(Path.Combine(ResolveDirectory.FullName, relativePath));
    }

    /// <summary>
    ///     Gets a directory from the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    /// <returns>The directory information.</returns>
    public DirectoryInfo GetDirectory(string relativePath)
    {
        return new DirectoryInfo(Path.Combine(ResolveDirectory.FullName, relativePath));
    }
}