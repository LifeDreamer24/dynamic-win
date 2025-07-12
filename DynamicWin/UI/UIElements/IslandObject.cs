using DynamicWin.Rendering.Primitives;
using DynamicWin.UI.Menu;
using DynamicWin.UserSettings;
using SkiaSharp;

namespace DynamicWin.UI.UIElements;

public class IslandObject : UIObject
{
    private readonly float[] _secondOrderValuesExpand = [2.5f, 0.6f, 0.1f];
    private readonly float[] _secondOrderValuesContract = [3f, 0.9f, 0.1f];
    
    private float topOffset { get; set; } = 7.5f;

    public SecondOrder scaleSecondOrder { get; }
    

    public bool Hidden { get; set; } = false;
        
    public Vec2 currSize;

    public enum IslandMode { Island, Notch };
    public IslandMode mode = Settings.IslandMode;

    float dropShadowStrength = 0f;
    float dropShadowSize = 0f;

    Color _borderColor = Color.Transparent;
    
    public event EventHandler? NeedsRendering;
    
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
        if (!Hidden)
        {
            var newPosY =  MathRendering.LinearInterpolation(LocalPosition.Y, topOffset, 15f * e.DeltaTime);
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
            var newPosY = MathRendering.LinearInterpolation(LocalPosition.Y, -Size.Y / 1.5f, 25f * e.DeltaTime);
            
            scaleSecondOrder.SetValues(_secondOrderValuesContract[0], _secondOrderValuesContract[1], _secondOrderValuesContract[2]);
            Size = scaleSecondOrder.Update(e.DeltaTime, new Vec2(500, 15));
            MainForm.Instance.Opacity = 0.85f;
            
            if (PaddingTop == newPosY)
            {
                return;
            }
            
            PaddingTop = newPosY;
        }
        
        NeedsRendering?.Invoke(this, EventArgs.Empty);
        mode = Settings.IslandMode;

        topOffset = MathRendering.LinearInterpolation(topOffset, (mode == IslandMode.Island) ? 7.5f : -2.5f, 15f * e.DeltaTime);

        dropShadowStrength = MathRendering.LinearInterpolation(dropShadowStrength, IsHovering ? 0.75f : 0.25f, 10f * e.DeltaTime);
        dropShadowSize = MathRendering.LinearInterpolation(dropShadowSize, IsHovering ? 35f : 7.5f, 10f * e.DeltaTime);

        _borderColor = Color.LinearInterpolation(_borderColor, MenuManager.Instance.ActiveMenu.IslandBorderColor(), 10f  * e.DeltaTime);
    }

    private static void Show(float deltaTime)
    {
        
    }

    public override void Draw(SKCanvas canvas)
    {
        var paint = GetPaint();
        paint.IsAntialias = Settings.AntiAliasing;

        paint.Color = Theme.IslandBackground.Value();

        if (!Hidden)
        {
            paint.ImageFilter = SKImageFilter.CreateDropShadow(1, 1, dropShadowSize, dropShadowSize, new Color(0, 0, 0).Override(alpha: dropShadowStrength).Value());
        }

        // Border
        var rect = GetRect();
        var paint2 = GetPaint();
        rect.Inflate(2.5f / 2, 2.5f / 2);
        paint2.Color = _borderColor.Override(alpha: _borderColor.Alpha * 0.35f).Value();
        paint2.IsStroke = true;
        paint2.StrokeWidth = 2.5f;

        canvas.DrawRoundRect(rect, paint2);

        paint.ImageFilter = null;

        if (mode == IslandMode.Notch && !Hidden)
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
    
    public void Mask(SKCanvas canvas)
    {
        var islandMask = GetMask();
        canvas.ClipRoundRect(islandMask);
    }

    private SKRoundRect GetMask()
    {
        var islandMask = GetRect();
        islandMask.Deflate(new SKSize(1, 1));
        return islandMask;
    }
}