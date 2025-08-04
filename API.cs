using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace RayUI;

public static partial class RUI
{
    public static ILogger Logger { get; set; } = NullLogger.Instance;
    public static Font DefaultFont { get; set; }

    public static void Init()
    {
        LoadShader();
        DefaultFont = GetFontDefault();
    }
    
    public static void Update(float dt, Vector2 size, params UIElement[] elements)
    {
        UpdateInput();

        foreach (var element in elements) element.SetSize(size);
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
        BeginShaderMode(shader);
        foreach (var element in elements) element.Render();
        EndShaderMode();
    }
}
