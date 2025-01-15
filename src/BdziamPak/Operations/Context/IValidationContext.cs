using BdziamPak.Operations.Steps;

namespace BdziamPak.Operations.Context;

/// <summary>
/// Interface for checking if a resolve step should be executed.
/// <see cref="BdziamPakOperationContext"/>
/// </summary>
public interface IValidationContext
{
    /// <summary>
    /// Checks if a specific resolve step was completed.
    /// </summary>
    /// <typeparam name="TStep">The type of the resolve step.</typeparam>
    /// <returns>true if the step was completed; otherwise, false.</returns>
    public bool WasCompleted<TStep>() where TStep : BdziamPakOperationStep;

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
    
    /// <summary>
    /// Gets a context parameter by name.
    /// </summary>
    /// <typeparam name="T">The type of the context parameter.</typeparam>
    /// <param name="name">The name of the context parameter.</param>
    /// <returns>The context parameter value if found; otherwise, the default value for the type.</returns>
    public T? GetContextParameter<T>(string name);
}