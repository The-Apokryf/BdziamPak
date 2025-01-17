using BdziamPak.Spectre.StepRendering.BuiltIn;

namespace BdziamPak.Spectre.StepRendering.Factory;

/// <summary>
/// A simple factory for selecting the appropriate renderer based on the type of the step progress model.
/// </summary>
public static class StepProgressRendererFactory
{
    private static readonly List<IStepProgressRenderer> _renderers = new List<IStepProgressRenderer>
    {
        new NuGetDownloadProgressRenderer(),
        new CloneRepositoryProgressRenderer()
    };

    /// <summary>
    /// Selects a renderer that can handle the provided progress model type.
    /// </summary>
    /// <param name="modelType">The type of the progress model.</param>
    /// <returns>An IStepProgressRenderer if found, otherwise null.</returns>
    public static IStepProgressRenderer? GetRendererForType(Type modelType)
    {
        return _renderers.FirstOrDefault(r => r.CanRender(modelType));
    }
}