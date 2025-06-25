using System.Globalization;

namespace DynamicWin.Rendering.Primitives;

public class Color(float red, float green, float blue, float alpha = 1f)
{
    public float Red { get; } = red;
    public float Green { get; } = green;
    public float Blue { get; } = blue;
    public float Alpha { get; } = alpha;

    public static Color White => new (1, 1, 1);
    public static Color Transparent => new (0, 0, 0, 0);

    public Color Override(float? red = null, float? green = null, float? blue = null, float? alpha = null)
    {
        var overrideRed = red ?? Red;
        var overrideGreen = green ?? Green;
        var overrideBlue = blue ?? Blue;
        var overrideAlpha = alpha ?? Alpha;

        return new Color(overrideRed, overrideGreen, overrideBlue, overrideAlpha);
    }

    public static Color LinearInterpolation(Color a, Color b, float t)
    {
        return new Color(
            MathRendering.LinearInterpolation(a.Red, b.Red, t),
            MathRendering.LinearInterpolation(a.Green, b.Green, t),
            MathRendering.LinearInterpolation(a.Blue, b.Blue, t),
            MathRendering.LinearInterpolation(a.Alpha, b.Alpha, t)
        ); ;
    }

    public SkiaSharp.SKColor Value()
    {
        return new SkiaSharp.SKColor(
            (byte)(Red * 255),
            (byte)(Green * 255),
            (byte)(Blue * 255),
            (byte)(Alpha * 255));
    }

    public Color Inverted()
    {
        return new Color(1f - Red, 1f - Green, 1f - Green, Alpha);
    }

    public static Color operator *(Color a, float b)
    {
        return new Color(a.Red * b, a.Green * b, a.Blue * b);
    }

    public static Color operator *(Color a, Color b)
    {
        return new Color(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue, a.Alpha * b.Alpha);
    }

    public static Color FromHex(string hex)
    {
        if (hex == null) return new Color(1, 0, 1);

        hex = hex.Replace("#", "");

        var hexCode = "";
        if (hex.Length == 6) hexCode += "ff";
        hexCode += hex;

        var argb = Int32.Parse(hexCode, NumberStyles.HexNumber);
        var clr = System.Drawing.Color.FromArgb(argb);

        return new Color(
            (float)clr.R / 255,
            (float)clr.G / 255,
            (float)clr.B / 255,
            (float)clr.A / 255
        );
    }
}