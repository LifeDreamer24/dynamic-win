using DynamicWin.Rendering.Primitives;
using DynamicWin.UserSettings;
using SkiaSharp;

namespace DynamicWin.UI.UIElements.Custom;

public class DWProgressBar : UIObject
{
    public float value = 1f;
    public float vaueSmoothing = 15f;

    public Color contentColor;

    public DWProgressBar(UIObject? parent, Vec2 position, Vec2 size, UIAlignment alignment = UIAlignment.TopCenter) : base(parent, position, size, alignment)
    {
        roundRadius = 15f;

        Color = Theme.IconColor.Override(alpha: 0.1f);
        contentColor = Theme.IconColor.Override(alpha: 1f);

        displayValue = value;
    }

    float displayValue = 1f;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        displayValue = MathRendering.LinearInterpolation(displayValue, value, vaueSmoothing * deltaTime);
    }

    public override void Draw(SKCanvas canvas)
    {
        var paint = GetPaint();

        var bgSize = Size;
        var bgP = RawPosition + LocalPosition;
        bgP.X += bgSize.X * (displayValue) + 3f;
        var bgRectPos = GetScreenPosFromRawPosition(bgP, bgSize);
        var bgRect = SKRect.Create(bgRectPos.X, bgRectPos.Y, bgSize.X * (1f - displayValue), bgSize.Y);
        var rBgRect = new SKRoundRect(bgRect, roundRadius);

        var fillSize = Size;
        var fillRectPos = GetScreenPosFromRawPosition(RawPosition + LocalPosition, fillSize);
        var fillRect = SKRect.Create(fillRectPos.X, fillRectPos.Y, fillSize.X * displayValue, fillSize.Y);
        var rFillRect = new SKRoundRect(fillRect, roundRadius);

        canvas.DrawRoundRect(rBgRect, paint);

        paint.Color = GetColor(contentColor).Value();
        canvas.DrawRoundRect(rFillRect, paint);
    }
}