using System.Drawing;
using System.Globalization;

namespace DynamicWin.Utils;

public class Color
{
    public float r, g, b, a;

    public static Color White { get => new Color(1, 1, 1); }
    public static Color Transparent { get => new Color(0, 0, 0, 0); }

    public Color(float r, float g, float b, float a = 1f)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public Color Override(float r = -1, float g = -1, float b = -1, float a = -1)
    {
        float red = this.r;
        float green = this.g;
        float blue = this.b;
        float alpha = this.a;

        if (r != -1) red = r;
        if (g != -1) green = g;
        if (b != -1) blue = b;
        if (a != -1) alpha = a;

        return new Color(red, green, blue, alpha);
    }

    public static Color Lerp(Color a, Color b, float t)
    {
        return new Color(
            Mathf.Lerp(a.r, b.r, t),
            Mathf.Lerp(a.g, b.g, t),
            Mathf.Lerp(a.b, b.b, t),
            Mathf.Lerp(a.a, b.a, t)
        ); ;
    }

    public SkiaSharp.SKColor Value()
    {
        return new SkiaSharp.SKColor(
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255),
            (byte)(a * 255));
    }

    public System.Drawing.Color ValueSystem()
    {
        return System.Drawing.Color.FromArgb(
            (byte)(a * 255),
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }

    public System.Windows.Media.Color ValueSystemMedia()
    {
        return System.Windows.Media.Color.FromArgb(
            (byte)(a * 255),
            (byte)(r * 255),
            (byte)(g * 255),
            (byte)(b * 255));
    }

    public Color Inverted()
    {
        return new Color(1f - r, 1f - g, 1f - g, a);
    }

    public static Color operator *(Color a, float b)
    {
        return new Color(a.r * b, a.g * b, a.b * b);
    }

    public static Color operator *(Color a, Color b)
    {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
    }

    public static Color FromHex(string hex)
    {
        if (hex == null) return new Color(1, 0, 1);

        hex = hex.Replace("#", "");

        string hexCode = "";
        if (hex.Length == 6) hexCode += "ff";
        hexCode += hex;

        int argb = Int32.Parse(hexCode, NumberStyles.HexNumber);
        System.Drawing.Color clr = System.Drawing.Color.FromArgb(argb);

        return new Color(
            (float)clr.R / 255,
            (float)clr.G / 255,
            (float)clr.B / 255,
            (float)clr.A / 255
        );
    }
}