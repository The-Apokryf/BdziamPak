using BdziamPak.PackageModel;

namespace BdziamPak.Operations.Context;

/// <summary>
///     Interface for executing operation context on the requested BdziamPak package.
///     <see cref="BdziamPakOperationContext" />
/// </summary>
public interface IExecuteOperationContext
{
    /// <summary>
    ///     Gets the metadata for the BdziamPak package.
    /// </summary>
    public BdziamPakMetadata BdziamPakMetadata { get; }

    /// <summary>
    ///     Gets the resolve directory.
    /// </summary>
    DirectoryInfo ResolveDirectory { get; }

    /// <summary>
    ///     Gets the metadata value for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the metadata value.</typeparam>
    /// <param name="key">The key of the metadata.</param>
    /// <returns>The metadata value if found; otherwise, the default value for the type.</returns>
    T? GetMetadata<T>(string key);

    /// <summary>
    ///     Sets a context parameter with the specified name and value.
    /// </summary>
    /// <typeparam name="T">The type of the context parameter value.</typeparam>
    /// <param name="name">The name of the context parameter.</param>
    /// <param name="value">The value of the context parameter.</param>
    void SetContextParameter<T>(string name, T value);


    /// <summary>
    ///     Gets a context parameter by name.
    /// </summary>
    /// <typeparam name="T">The type of the context parameter.</typeparam>
    /// <param name="name">The name of the context parameter.</param>
    /// <returns>The context parameter value if found; otherwise, the default value for the type.</returns>
    public T? GetContextParameter<T>(string name);
}