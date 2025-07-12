﻿using DynamicWin.Rendering.Primitives;
using DynamicWin.UserSettings;
using SkiaSharp;

namespace DynamicWin.UI.UIElements.Custom;

internal class DropFileElement : UIObject
{
    public DropFileElement(UIObject? parent, Vec2 position, Vec2 size, string displayText = "Drop Files to Tray", int tSize = 24, UIAlignment alignment = UIAlignment.TopCenter) : base(parent, position, size, alignment)
    {
        roundRadius = 25;

        AddLocalObject(new DWText(null, displayText, Vec2.zero, UIAlignment.Center) { Font = Resources.FileResources.InterBold, TextSize = tSize });
    }

    Color _currentColor = Theme.Secondary;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        _currentColor = Color.LinearInterpolation(_currentColor, IsHovering ? Theme.Secondary * 2f : Theme.Secondary, 5f * deltaTime);
    }

    public override void Draw(SKCanvas canvas)
    {
        var paint = GetPaint();
        var rect = GetRect();

        float[] intervals = { 10, 10 };
        paint.PathEffect = SKPathEffect.CreateDash(intervals, 0f);

        paint.IsStroke = true;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.StrokeJoin = SKStrokeJoin.Round;
        paint.StrokeWidth = 2f;

        paint.Color = GetColor(Theme.Primary).Value();

        canvas.DrawRoundRect(rect, paint);

        paint.Color = GetColor(_currentColor).Value();
        paint.IsStroke = false;

        rect.Deflate(10, 10);

        canvas.DrawRoundRect(rect, paint);
    }
}