using Spectre.Console;
using Spectre.Console.Rendering;

namespace BdziamPak.Spectre;

public class OperationRenderer
{
    protected IRenderable Render(object itemToRender) => new Markup(itemToRender.ToString());
}