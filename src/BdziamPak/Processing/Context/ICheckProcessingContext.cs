namespace BdziamPak.Resolving.ResolveSteps;

/// <summary>
/// Interface for checking if a resolve step should be executed.
/// <see cref="BdziamPakProcessingContext"/>
/// </summary>
public interface ICheckProcessingContext
{
    /// <summary>
    /// Checks if a specific resolve step was completed.
    /// </summary>
    /// <typeparam name="TStep">The type of the resolve step.</typeparam>
    /// <returns>true if the step was completed; otherwise, false.</returns>
    public bool WasCompleted<TStep>() where TStep : BdziamPakProcessStep;

    /// <summary>
    /// Checks if a file exists in the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
    public bool FileExists(string relativePath);

    /// <summary>
    /// Checks if a directory exists in the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    /// <returns>true if the directory exists; otherwise, false.</returns>
    public bool DirectoryExists(string relativePath);

    /// <summary>
    /// Checks if the metadata contains a specific key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>true if the metadata contains the key; otherwise, false.</returns>
    public bool HasMetadata(string key);

    /// <summary>
    /// Gets the metadata value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="key">The key of the metadata.</param>
    /// <returns>The metadata value if found; otherwise, the default value for the type.</returns>
    public T? GetMetadata<T>(string key);
}