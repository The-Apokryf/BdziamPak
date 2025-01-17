using Spectre.Console;

namespace BdziamPak.Spectre.StepRendering;

/// <summary>
/// A base class that adapts a strongly typed renderer into a non-generic interface.
/// </summary>
/// <typeparam name="T">The type of progress model.</typeparam>
public abstract class StepProgressRendererBase<T> : IStepProgressRenderer
{
    /// <summary>
    /// Determines whether this renderer can render a specific model type.
    /// </summary>
    public bool CanRender(Type type) => type == typeof(T);

    /// <summary>
    /// Renders the step progress as an object by casting.
    /// </summary>
    public void Render(object model, IAnsiConsole console)
    {
        if (model is T typedModel)
        {
            Render(typedModel, console);
        }
    }

    /// <inheritdoc />
    public abstract void Render(T model, IAnsiConsole console);
}