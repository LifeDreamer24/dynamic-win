using DynamicWin.Main;
using DynamicWin.Utils;
using SkiaSharp;
using System.Windows.Controls;

namespace DynamicWin.UI.Widgets;

public class WidgetBase : UIObject
{
    private float _hoverProgress;
    
    public bool IsEditMode { get; set; }

    public WidgetBase(UIObject? parent, Vec2 position, UIAlignment alignment = UIAlignment.TopCenter) : base(parent, position, Vec2.zero, alignment)
    {
        Size = GetWidgetSize();

        var objs = InitializeWidget();
        objs.ForEach(obj => AddLocalObject(obj));

        roundRadius = 15f;
    }

    public Vec2 GetWidgetSize() { return new Vec2(GetWidgetWidth(), GetWidgetHeight()); }

    protected virtual float GetWidgetHeight() { return 100; }
    protected virtual float GetWidgetWidth() { return 200; }

    public List<UIObject> InitializeWidget()
    {
        return new List<UIObject>();
    }

    public Action onEditRemoveWidget;
    public Action onEditMoveWidgetLeft;
    public Action onEditMoveWidgetRight;

    public override ContextMenu? GetContextMenu()
    {
        if (!IsEditMode) return null;

        var ctx = new ContextMenu();

        MenuItem remove = new MenuItem() { Header = "Remove" };
        remove.Click += (x, y) => onEditRemoveWidget?.Invoke();

        MenuItem pL = new MenuItem() { Header = "<- Push Left" };
        pL.Click += (x, y) => onEditMoveWidgetLeft?.Invoke();

        MenuItem pR = new MenuItem() { Header = "Push Right ->" };
        pR.Click += (x, y) => onEditMoveWidgetRight?.Invoke();
            
        ctx.Items.Add(remove);
        ctx.Items.Add(pL);
        ctx.Items.Add(pR);

        return ctx;
    }

    public override void Draw(SKCanvas canvas)
    {
        Size = GetWidgetSize();

        _hoverProgress = Mathf.Lerp(_hoverProgress, IsHovering ? 1f : 0f, 10f * RendererMain.Instance.DeltaTime);

        if(_hoverProgress > 0.025f)
        {
            var paintOnHovering = GetPaint();
            paintOnHovering.ImageFilter = SKImageFilter.CreateDropShadowOnly(
                dx: 0, 
                dy: 0, 
                sigmaX: _hoverProgress * 10, 
                sigmaY: _hoverProgress * 10, 
                color: Theme.WidgetBackground.Override(a: _hoverProgress / 10).Value());

            canvas.Save();

            var p = Position + Size / 2;
            canvas.Scale(1 + _hoverProgress / 60, 1 + _hoverProgress / 60, p.X, p.Y);

            var sc = canvas.Save();
            canvas.ClipRoundRect(GetRect(), SKClipOperation.Difference, antialias: true);
            canvas.DrawRoundRect(GetRect(), paintOnHovering);
            canvas.RestoreToCount(sc);
        }
        
        DrawWidget(canvas);

        if (!IsEditMode) return;
        
        var paint = GetPaint();

        paint.IsStroke = true;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.StrokeJoin = SKStrokeJoin.Round;
        paint.StrokeWidth = 2f;

        const float expand = 10;
        var brect = SKRect.Create(Position.X - expand / 2, Position.Y - expand / 2, Size.X + expand, Size.Y + expand);
        var broundRect = new SKRoundRect(brect, roundRadius);

        var noClip = canvas.Save();

        paint.Color = SKColors.DimGray;
        canvas.DrawRoundRect(broundRect, paint);

        canvas.RestoreToCount(noClip);
    }

    protected virtual void DrawWidget(SKCanvas canvas) { }
}