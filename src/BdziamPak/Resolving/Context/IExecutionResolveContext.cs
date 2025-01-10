using BdziamPak.Packages.Packaging.Model;

namespace BdziamPak.Resolving;

/// <summary>
/// Interface for executing resolve context in the BdziamPak resolving process.
/// <see cref="BdziamPakResolveContext"/>
/// </summary>
public interface IExecutionResolveContext
{
    /// <summary>
    /// Gets the metadata for the BdziamPak package.
    /// </summary>
    public BdziamPakMetadata BdziamPakMetadata { get; }

    /// <summary>
    /// Gets the resolve directory.
    /// </summary>
    DirectoryInfo ResolveDirectory { get; }

    /// <summary>
    /// Marks the resolve process as failed with a message.
    /// </summary>
    /// <param name="message">The failure message.</param>
    void Fail(string message);

    /// <summary>
    /// Skips the current step with a message.
    /// </summary>
    /// <param name="message">The skip message.</param>
    void Skip(string message);

    /// <summary>
    /// Completes the current resolve step.
    /// </summary>
    void Complete();

    /// <summary>
    /// Updates the resolve status with a message and optional percentage.
    /// </summary>
    /// <param name="message">The status message.</param>
    /// <param name="percent">The optional percentage.</param>
    void UpdateStatus(string message, int? percent = null);

    /// <summary>
    /// Gets a file from the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>The file information.</returns>
    FileInfo GetFile(string relativePath);

    /// <summary>
    /// Gets a directory from the resolve directory.
    /// </summary>
    /// <param name="relativePath">The relative path of the directory.</param>
    /// <returns>The directory information.</returns>
    DirectoryInfo GetDirectory(string relativePath);

    /// <summary>
    /// Gets the metadata value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="key">The key of the metadata.</param>
    /// <returns>The metadata value if found; otherwise, the default value for the type.</returns>
    public T? GetMetadata<T>(string key);
}