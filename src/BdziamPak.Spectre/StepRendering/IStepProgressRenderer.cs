using Spectre.Console;

/// <summary>
/// A typed renderer interface for step progress models of type T.
/// </summary>
public interface IStepProgressRenderer
{
    /// <summary>
    /// Determines whether this renderer can render a specific model type.
    /// </summary>
    /// <param name="type">The model type to check.</param>
    /// <returns>True if renderer can handle the type, false otherwise.</returns>
    bool CanRender(Type type);
    /// <summary>
    /// Renders the step progress using Spectre.Console.
    /// </summary>
    /// <param name="model">The progress model to render.</param>
    /// <param name="console">The console to write output to.</param>
    void Render(object model, IAnsiConsole console);
}