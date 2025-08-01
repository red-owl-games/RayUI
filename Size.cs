namespace Raylib.UI;

public struct Size
{
    public enum Mode
    {
        Value,
        Grow,
        Fit,
    }

    public Mode Type;
    public float Value;

    public static Size Fixed(float value) => new() { Type = Mode.Value, Value = value };
    public static Size Zero() => Fixed(0);
    public static Size One() => Fixed(1);
    public static Size Grow(float value = 1) => new() { Type = Mode.Grow, Value = value };
    public static Size Fit(float value = 1) => new() { Type = Mode.Fit, Value = value };
}
