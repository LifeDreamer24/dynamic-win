namespace DynamicWin.UI;

public enum UIAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    Center,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
    None
}

/*public static class UIAlignmentExtensions
{
    public static Vec2 GetPosition(this UIAlignment alignment, Vec2 size)
    {
        return alignment switch
        {
            UIAlignment.TopLeft => new Vec2(0, 0),
            UIAlignment.TopCenter => new Vec2(-size.X / 2, 0),
            UIAlignment.TopRight => new Vec2(-size.X, 0),
            UIAlignment.MiddleLeft => new Vec2(0, -size.Y / 2),
            UIAlignment.Center => new Vec2(-size.X / 2, -size.Y / 2),
            UIAlignment.MiddleRight => new Vec2(-size.X, -size.Y / 2),
            UIAlignment.BottomLeft => new Vec2(0, -size.Y),
            UIAlignment.BottomCenter => new Vec2(-size.X / 2, -size.Y),
            UIAlignment.BottomRight => new Vec2(-size.X, -size.Y),
            _ => new Vec2(0, 0)
        };
    }
}

internal interface UIAlignment*/