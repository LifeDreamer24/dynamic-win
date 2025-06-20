using DynamicWin.Main;
using DynamicWin.UI.Menu;
using DynamicWin.Utils;
using FftSharp;
using SkiaSharp;

namespace DynamicWin.UI.UIElements;

public class IslandObject : UIObject
{
    private readonly float[] _secondOrderValuesExpand = [2.5f, 0.6f, 0.1f];
    private readonly float[] _secondOrderValuesContract = [3f, 0.9f, 0.1f];
    
    private float topOffset { get; set; } = 15f;

    public SecondOrder scaleSecondOrder { get; }
    

    public bool hidden { get; set; } = false;
        
    public Vec2 currSize;

    public enum IslandMode { Island, Notch };
    public IslandMode mode = Settings.IslandMode;

    float dropShadowStrength = 0f;
    float dropShadowSize = 0f;

    Color _borderColor = Utils.Color.Transparent;

    public IslandObject() : base(null, Vec2.zero, new Vec2(250, 50))
    {
        currSize = Size;

        Anchor = new Vec2(0.5f, 0f);

        roundRadius = 35f;

        LocalPosition = new Vec2(0, topOffset);
        PaddingTop = topOffset;

        scaleSecondOrder = new SecondOrder(Size, _secondOrderValuesExpand[0], _secondOrderValuesExpand[1], _secondOrderValuesExpand[2]);
        expandInteractionRect = 20;

        maskInToIsland = false;
        
        

        MouseEnter += UIObject_MouseEnter;
        MouseLeave += UIObject_MouseLeave;
        IsRendering += UIObject_IsRendering;
    }
    
    private void UIObject_MouseEnter(object? sender, EventArgs e)
    {
        scaleSecondOrder.SetValues(_secondOrderValuesExpand[0], _secondOrderValuesExpand[1], _secondOrderValuesExpand[2]);
        currSize = MenuManager.Instance.ActiveMenu.IslandSizeBig();
    }
    
    private void UIObject_MouseLeave(object? sender, EventArgs e)
    {
        scaleSecondOrder.SetValues(_secondOrderValuesContract[0], _secondOrderValuesContract[1], _secondOrderValuesContract[2]);
        currSize = MenuManager.Instance.ActiveMenu.IslandSize();
    }

    private void UIObject_IsRendering(object? sender, IsRenderingEventArgs e)
    {
        if (!hidden)
        {
            var newPosY =  Mathf.Lerp(LocalPosition.Y, topOffset, 15f * e.DeltaTime);
            Size = scaleSecondOrder.Update(e.DeltaTime, currSize);
            
            MainForm.Instance.Opacity = 1f;
            if (PaddingTop == newPosY)
            {
                return;
            }
            
            PaddingTop = newPosY;
            
        }
        else
        {
            var newPosY = Mathf.Lerp(LocalPosition.Y, -Size.Y / 1.5f, 25f * e.DeltaTime);
            
            scaleSecondOrder.SetValues(_secondOrderValuesContract[0], _secondOrderValuesContract[1], _secondOrderValuesContract[2]);
            Size = scaleSecondOrder.Update(e.DeltaTime, new Vec2(500, 15));
            MainForm.Instance.Opacity = 0.85f;
            
            if (PaddingTop == newPosY)
            {
                return;
            }
            
            PaddingTop = newPosY;
        }

        mode = Settings.IslandMode;

        topOffset = Mathf.Lerp(topOffset, (mode == IslandMode.Island) ? 7.5f : -2.5f, 15f * e.DeltaTime);

        dropShadowStrength = Mathf.Lerp(dropShadowStrength, IsHovering ? 0.75f : 0.25f, 10f * e.DeltaTime);
        dropShadowSize = Mathf.Lerp(dropShadowSize, IsHovering ? 35f : 7.5f, 10f * e.DeltaTime);

        _borderColor = Utils.Color.Lerp(_borderColor, MenuManager.Instance.ActiveMenu.IslandBorderColor(), 10f  * e.DeltaTime);
    }

    private static void Show(float deltaTime)
    {
        
    }

    public override void Draw(SKCanvas canvas)
    {
        var paint = GetPaint();
        paint.IsAntialias = Settings.AntiAliasing;

        paint.Color = Theme.IslandBackground.Value();

        if (!hidden)
        {
            paint.ImageFilter = SKImageFilter.CreateDropShadow(1, 1, dropShadowSize, dropShadowSize, new Color(0, 0, 0).Override(a: dropShadowStrength).Value());
        }

        // Border
        var rect = GetRect();
        var paint2 = GetPaint();
        rect.Inflate(2.5f / 2, 2.5f / 2);
        paint2.Color = _borderColor.Override(a: _borderColor.a * 0.35f).Value();
        paint2.IsStroke = true;
        paint2.StrokeWidth = 2.5f;

        canvas.DrawRoundRect(rect, paint2);

        paint.ImageFilter = null;

        if (mode == IslandMode.Notch && !hidden)
        {
            var path = new SKPath();

            var awidth = (float)(Math.Max(0f, Size.Y / 45));
            var aheight = (float)(Math.Max(Size.Y / 45, 15)) + (LocalPosition.Y - topOffset);
            var y = 0;

            { // Left notch curve

                var x = Position.X - awidth;

                path.MoveTo(x - awidth, y);
                path.CubicTo(
                    x + 0, y,
                    x + awidth, y,
                    x + awidth, y + aheight);
                path.LineTo(x + awidth, y);
                path.LineTo(x + 0, y);
            }

            { // Right notch curve

                var x = Position.X + Size.X + awidth;

                path.MoveTo(x + awidth, y);
                path.CubicTo(
                    x - 0, y,
                    x - awidth, y,
                    x - awidth, y + aheight);
                path.LineTo(x - awidth, y);
                path.LineTo(x - 0, y);
            }

            var r = SKRect.Create(Position.X, 0, Size.X, (Position.Y - topOffset) + topOffset + Size.Y / 2);
            path.AddRect(r);

            canvas.DrawPath(path, paint);
        }

        canvas.DrawRoundRect(GetRect(), paint);
    }

    public override SKRoundRect GetInteractionRect()
    {
        var rect = SKRect.Create(Position.X, Position.Y, Size.X, Size.Y);

        if(IsHovering)
            rect.Inflate(expandInteractionRect + 5, expandInteractionRect + 5);

        rect.Inflate(expandInteractionRect, expandInteractionRect);
        var r = new SKRoundRect(rect, roundRadius);

        return r;
    }

    public override SKRoundRect GetRect()
    {
        var rect = base.GetRect();
        return rect;
    }
}