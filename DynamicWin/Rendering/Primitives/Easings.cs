namespace DynamicWin.Rendering.Primitives;

public static class Easings
{
    public static float EaseInQuint(float x)
    {
        return x * x * x * x * x;
    }

    public static float EaseOutQuint(float x)
    {
        return 1 - (float)Math.Pow(1 - x, 5);
    }

    public static float EaseOutSin(float x)
    {
        return (float)Math.Sin((x * Math.PI) / 2);
    }

    public static float EaseInSin(float x)
    {
        return 1 - (float)Math.Cos((x * Math.PI) / 2);
    }

    public static float EaseOutCubic(float x)
    {
        return 1 - (float)Math.Pow(1 - x, 3);
    }

    public static float EaseInCubic(float x)
    {
        return x * x * x;
    }
}