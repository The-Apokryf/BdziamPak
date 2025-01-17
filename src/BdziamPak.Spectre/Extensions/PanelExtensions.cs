using Spectre.Console;
using Spectre.Console.Rendering;

namespace BdziamPak.Spectre.Extensions;

public static class PanelExtensions
{
    public static Panel SetContent(this Panel panel, IRenderable newContent)
    {
        // Clone the settings of the existing panel
        var newPanel = new Panel(newContent)
        {
            Header = panel.Header,
            Border = panel.Border,
            BorderStyle = panel.BorderStyle,
            Padding = panel.Padding,
            Width = panel.Width,
            Height = panel.Height,
            Expand = panel.Expand
        };

        return newPanel;
    }
}