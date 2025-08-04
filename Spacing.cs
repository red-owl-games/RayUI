namespace Raylib.UI;

public struct Spacing(float top, float right, float bottom, float left)
{
    public float Top = top;
    public float Right = right;
    public float Bottom = bottom;
    public float Left = left;

    public float Width => right + left;
    public float Height => top + bottom;

    public static Spacing Zero() => New(0);
    
    ///New 1 - (All), 2 (Top/Bottom, Right/Left), 4 (Top, Right, Bottom, Left)
    public static Spacing New(params float[] values)
    {
        return values.Length switch
        {
            1 => new Spacing(values[0], values[0], values[0], values[0]),
            2 => new Spacing(values[0], values[1], values[0], values[1]),
            4 => new Spacing(values[0], values[1], values[2], values[3]),
            _ => throw new ArgumentOutOfRangeException(nameof(values), "Spacing expected 1, 2 or 4 values")
        };
    }
}
