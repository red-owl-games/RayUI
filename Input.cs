using System.Numerics;
using static Raylib_cs.Raylib;

namespace Raylib.UI;

public static partial class RUI
{
    public static Vector2 MousePosition {get; private set;}

    private static void UpdateInput()
    {
        MousePosition = GetMousePosition();
    }
}
