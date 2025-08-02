using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Raylib.UI;

public static partial class RUI
{
    public static ILogger Logger { get; set; } = NullLogger.Instance;
    
    public static void Update(float dt, Vector2 size, params UIElement[] elements)
    {
        UpdateInput();

        foreach (var element in elements) element.LayoutFitWidth(size);
        foreach (var element in elements) element.LayoutGrowShrinkWidth(size);
        foreach (var element in elements) element.LayoutWrapText(size);
        foreach (var element in elements) element.LayoutFitHeight(size);
        foreach (var element in elements) element.LayoutGrowShrinkHeight(size);
        foreach (var element in elements) element.CalculatePosition(Vector2.Zero);
        foreach (var element in elements) element.Update(dt);
    }

    public static void Render(params UIElement[] elements)
    {
        foreach (var element in elements) element.Render();
    }
}
