using SkiaSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DynamicWin.Rendering;
using DynamicWin.Rendering.Primitives;
using DynamicWin.UserSettings;

namespace DynamicWin.UI;

public class IsRenderingEventArgs(float deltaTime) : EventArgs
{
    public float DeltaTime { get; } = deltaTime;
}

public class UIObject
{
    private UIObject? parent;
    public UIObject? Parent { get { return parent; } set { parent = value; } }

    private Vec2 position = Vec2.zero;
    private Vec2 localPosition = Vec2.zero;
    private Vec2 anchor = new Vec2(0.5f, 0.5f);
    private Vec2 size = Vec2.one;
    private Color color = Color.White;
    
    private bool? _needsRendering = null;

    public Vec2 RawPosition { get => position; }
    public Vec2 Position { get => GetPosition() + localPosition; set => position = value; }
    public Vec2 LocalPosition { get => localPosition; set => localPosition = value; }
    public Vec2 Anchor { get => anchor; set => anchor = value; }

    // TODO: look further into this null pointer issue
    public Vec2 Size
    {
        get => size;
        set
        {
            // var yDiff = float.Abs(size.Y - value.Y);
            // var xDiff = float.Abs(size.X - value.X);
            
            size = value;
        }
    }
    // Temporary fix for null size especially with BottomLeft getter
    public Color Color { get => new Color(color.Red, color.Green, color.Blue, color.Alpha * Alpha); set => color = value; }

    private bool isHovering = false;
    private bool isMouseDown = false;
    private bool isGlobalMouseDown = false;
    protected bool drawLocalObjects = true;

    public bool IsHovering
    {
        get => isHovering;
        private set
        {
            isHovering = value;
        }
    }

    private bool _isHoveringChanged = false;

    public bool IsMouseDown { get => isMouseDown; private set => isMouseDown = value; }

    public UIAlignment alignment = UIAlignment.TopCenter;

    protected float localBlurAmount = 0f;
    public float blurAmount = 0f;
    public float roundRadius = 0f;
    public bool maskInToIsland = true;

    private List<UIObject> localObjects = new List<UIObject>();
    public List<UIObject> LocalObjects { get => localObjects; }

    private bool isEnabled = true;
    public bool IsEnabled { get => isEnabled; set => SetActive(value); }

    public float blurSizeOnDisable = 50;

    private float pAlpha = 1f;
    private float oAlpha = 1f;

    public float Alpha { get => (float) Math.Min(pAlpha, Math.Min(oAlpha, DynamicWinRenderer.Instance.AlphaOverride)); set => oAlpha = value; }

    public float PaddingTop {
        get => LocalPosition.Y;
        set => LocalPosition.Y = value;
    }

    protected void AddLocalObject(UIObject obj)
    {
        obj.parent = this;
        localObjects.Add(obj);
    }

    protected void DestroyLocalObject(UIObject obj)
    {
        obj.DestroyCall();
        localObjects.Remove(obj);
    }
    
    public event EventHandler? MouseEnter;
    public event EventHandler? MouseLeave;
    public event EventHandler<IsRenderingEventArgs>? IsRendering;

    public UIObject(UIObject? parent, Vec2 position, Vec2 size, UIAlignment alignment = UIAlignment.TopCenter)
    {
        this.parent = parent;
        this.position = position;
        this.size = size;
        this.alignment = alignment;

        this.contextMenu = CreateContextMenu();

        DynamicWinRenderer.Instance.ContextMenuOpening += CtxOpen;
        DynamicWinRenderer.Instance.ContextMenuClosing += CtxClose;
    }
    
    public UIObject(Vec2 position, Vec2 size, UIAlignment alignment = UIAlignment.TopCenter, List<UIObject>? children = null)
    {
        this.position = position;
        this.size = size;
        this.alignment = alignment;

        foreach (var child in children ?? [])
        {
            child.Parent = this;
        }

        contextMenu = CreateContextMenu();

        DynamicWinRenderer.Instance.ContextMenuOpening += CtxOpen;
        DynamicWinRenderer.Instance.ContextMenuClosing += CtxClose;
    }
    
    public void AddChild(UIObject child)
    {
        if (child == null) return;

        child.parent = this;
        //localObjects.Add(child);
    }

    void CtxOpen(object sender, RoutedEventArgs e)
    {
        if(DynamicWinRenderer.Instance.ContextMenu != null)
            canInteract = false;
    }

    void CtxClose(object sender, RoutedEventArgs e)
    {
        canInteract = true;
    }

    public Vec2 GetScreenPosFromRawPosition(Vec2 position, Vec2 Size = null, UIAlignment alignment = UIAlignment.None, UIObject parent = null)
    {
        if (parent == null) parent = this.parent;
        if (Size == null) Size = this.Size;
        if (alignment == UIAlignment.None) alignment = this.alignment;

        if (parent == null)
        {
            Vec2 screenDim = DynamicWinRenderer.ScreenDimensions;
            if (Size == null) Size = Vec2.one;
            switch (alignment)
            {
                case UIAlignment.TopLeft:
                    return new Vec2(position.X - (Size.X * Anchor.X),
                        position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopCenter:
                    return new Vec2(position.X + screenDim.X / 2 - (Size.X * Anchor.X),
                        position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopRight:
                    return new Vec2(position.X + screenDim.X - (Size.X * Anchor.X),
                        position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleLeft:
                    return new Vec2(position.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.Center:
                    return new Vec2(position.X + screenDim.X / 2 - (Size.X * Anchor.X),
                        position.Y + screenDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleRight:
                    return new Vec2(position.X + screenDim.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.BottomLeft:
                    return new Vec2(position.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomCenter:
                    return new Vec2(position.X + screenDim.X / 2 - (Size.X * Anchor.X),
                        position.Y + screenDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomRight:
                    return new Vec2(position.X + screenDim.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y - (Size.Y * Anchor.Y));
            }
        }
        else
        {
            Vec2 parentDim = parent.Size;
            Vec2 parentPos = parent.Position;

            switch (alignment)
            {
                case UIAlignment.TopLeft:
                    return new Vec2(parentPos.X + position.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopCenter:
                    return new Vec2(parentPos.X + position.X + parentDim.X / 2 - (Size.X * Anchor.X),
                        parentPos.Y + position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopRight:
                    return new Vec2(parentPos.X + position.X + parentDim.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleLeft:
                    return new Vec2(parentPos.X + position.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.Center:
                    return new Vec2(parentPos.X + position.X + parentDim.X / 2 - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleRight:
                    return new Vec2(parentPos.X + position.X + parentDim.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.BottomLeft:
                    return new Vec2(parentPos.X + position.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomCenter:
                    return new Vec2(parentPos.X + position.X + parentDim.X / 2 - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomRight:
                    return new Vec2(parentPos.X + position.X + parentDim.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y - (Size.Y * Anchor.Y));
            }
        }

        return Vec2.zero;
    }

    protected virtual Vec2 GetPosition()
    {
        if(parent == null)
        {
            Vec2 screenDim = DynamicWinRenderer.ScreenDimensions;
            switch (alignment)
            {
                case UIAlignment.TopLeft:
                    return new Vec2(position.X - (Size.X * Anchor.X),
                        position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopCenter:
                    return new Vec2(position.X + screenDim.X / 2 - (Size.X * Anchor.X),
                        position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopRight:
                    return new Vec2(position.X + screenDim.X - (Size.X * Anchor.X),
                        position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleLeft:
                    return new Vec2(position.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.Center:
                    return new Vec2(position.X + screenDim.X / 2 - (Size.X * Anchor.X),
                        position.Y + screenDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleRight:
                    return new Vec2(position.X + screenDim.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.BottomLeft:
                    return new Vec2(position.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomCenter:
                    return new Vec2(position.X + screenDim.X / 2 - (Size.X * Anchor.X),
                        position.Y + screenDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomRight:
                    return new Vec2(position.X + screenDim.X - (Size.X * Anchor.X),
                        position.Y + screenDim.Y - (Size.Y * Anchor.Y));
            }
        }
        else
        {
            Vec2 parentDim = parent.Size;
            Vec2 parentPos = parent.Position;

            switch (alignment)
            {
                case UIAlignment.TopLeft:
                    return new Vec2(parentPos.X + position.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopCenter:
                    return new Vec2(parentPos.X + position.X + parentDim.X / 2 - (Size.X * Anchor.X),
                        parentPos.Y + position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.TopRight:
                    return new Vec2(parentPos.X + position.X + parentDim.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleLeft:
                    return new Vec2(parentPos.X + position.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.Center:
                    return new Vec2(parentPos.X + position.X + parentDim.X / 2 - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.MiddleRight:
                    return new Vec2(parentPos.X + position.X + parentDim.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y / 2 - (Size.Y * Anchor.Y));
                case UIAlignment.BottomLeft:
                    return new Vec2(parentPos.X + position.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomCenter:
                    return new Vec2(parentPos.X + position.X + parentDim.X / 2 - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y - (Size.Y * Anchor.Y));
                case UIAlignment.BottomRight:
                    return new Vec2(parentPos.X + position.X + parentDim.X - (Size.X * Anchor.X),
                        parentPos.Y + position.Y + parentDim.Y - (Size.Y * Anchor.Y));
            }
        }

        return Vec2.zero;
    }

    public float GetBlur()
    {
        if (!Settings.AllowBlur) return 0f;
        return Math.Max(blurAmount, Math.Max(localBlurAmount, Math.Max((parent == null) ? 0f : parent.GetBlur(), DynamicWinRenderer.Instance.BlurOverride)));
    }

    bool canInteract = true;
        
    public void UpdateCall(float deltaTime)
    {
        if (!isEnabled) return;

        if (parent != null)
            pAlpha = parent.Alpha;

        if (canInteract)
        {
            var rect = SKRect.Create(DynamicWinRenderer.CursorPosition.X, DynamicWinRenderer.CursorPosition.Y, 1, 1);
            var isCurrentlyHovering = GetInteractionRect().Contains(rect);
            var hasHoveringChanged = isHovering != isCurrentlyHovering;

            isHovering = isCurrentlyHovering;
            
            if (isCurrentlyHovering)//isHovering && hasHoveringChanged)
            {
                MouseEnter?.Invoke(this, EventArgs.Empty);
            }

            if (!isCurrentlyHovering)//!isHovering && hasHoveringChanged)
            {
                MouseLeave?.Invoke(this, EventArgs.Empty);
            }

            if (!isGlobalMouseDown && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                isGlobalMouseDown = true;
                OnGlobalMouseDown();
            }
            else if (isGlobalMouseDown && !(Mouse.LeftButton == MouseButtonState.Pressed))
            {
                isGlobalMouseDown = false;
                OnGlobalMouseUp();
            }

            if (IsHovering && !IsMouseDown && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IsMouseDown = true;
                OnMouseDown();
            }
            else if (IsHovering && IsMouseDown && !(Mouse.LeftButton == MouseButtonState.Pressed))
            {
                IsMouseDown = false;
                OnMouseUp();
            }
            else if (IsMouseDown && !(Mouse.LeftButton == MouseButtonState.Pressed))
            {
                IsMouseDown = false;
            }
        }
        

        IsRendering?.Invoke(this, new IsRenderingEventArgs(deltaTime));
        
        Update(deltaTime);

        if (drawLocalObjects)
        {
            new List<UIObject>(localObjects).ForEach((UIObject obj) =>
            {
                obj.blurAmount = GetBlur();
                obj.UpdateCall(deltaTime);
            });
        }
    }

    public virtual void Update(float deltaTime) { }

    public void DrawCall(SKCanvas canvas)
    {
        if (!isEnabled) return;

        Draw(canvas);

        if (drawLocalObjects)
        {
            new List<UIObject>(localObjects).ForEach((UIObject obj) =>
            {
                obj.DrawCall(canvas);
            });
        }
    }

    public virtual void Draw(SKCanvas canvas)
    {
        var rect = SKRect.Create(Position.X, Position.Y, Size.X, Size.Y);
        var roundRect = new SKRoundRect(rect, roundRadius);

        var paint = GetPaint();

        canvas.DrawRoundRect(roundRect, paint);
    }

    public virtual SKPaint GetPaint()
    {
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = this.Color.Value(),
            IsAntialias = Settings.AntiAliasing,
            IsDither = true,
            SubpixelText = false,
            FilterQuality = SKFilterQuality.Medium,
            HintingLevel = SKPaintHinting.Normal,
            IsLinearText = true
        };

        if(GetBlur() != 0f)
        {
            var blur = SKImageFilter.CreateBlur(GetBlur(), GetBlur());
            paint.ImageFilter = blur;
        }

        return paint;
    }

    public Color GetColor(Color color)
    {
        return new Color(color.Red, color.Green, color.Blue, color.Alpha * Alpha);
    }

    public void DestroyCall() 
    {
        localObjects.ForEach((UIObject obj) =>
        {
            obj.DestroyCall();
        });

        OnDestroy();
    }

    public virtual void OnDestroy() { }

    public virtual void OnMouseDown() { }
    public virtual void OnGlobalMouseDown() { }

    public virtual void OnMouseUp() { }
    public virtual void OnGlobalMouseUp() { }

    public void SilentSetActive(bool isEnabled)
    {
        this.isEnabled = isEnabled;
    }

    Animator toggleAnim;
    bool lastSetActiveCall = true;

    public void SetActive(bool isEnabled)
    {
        if(this.isEnabled == isEnabled && lastSetActiveCall == isEnabled) return;
        if (toggleAnim != null && toggleAnim.IsRunning) toggleAnim.Stop();

        lastSetActiveCall = isEnabled;

        if (isEnabled)
        {
            localBlurAmount = blurSizeOnDisable;
            Alpha = 0f;
            this.isEnabled = isEnabled;
        }

        toggleAnim = new Animator(250, 1);
        toggleAnim.onAnimationUpdate += (t) =>
        {
            if(t >= 0.5f) this.isEnabled = isEnabled;

            if (isEnabled)
            {
                var tEased = Easings.EaseOutCubic(t);

                localBlurAmount = MathRendering.LinearInterpolation(blurSizeOnDisable, 0, tEased);
                Alpha = MathRendering.LinearInterpolation(0, 1, tEased);
            }
            else
            {
                var tEased = Easings.EaseOutCubic(t);

                localBlurAmount = MathRendering.LinearInterpolation(0, blurSizeOnDisable, tEased);
                Alpha = MathRendering.LinearInterpolation(1, 0, tEased);
            }
        };
        toggleAnim.onAnimationEnd += () =>
        {
            this.isEnabled = isEnabled;
            localBlurAmount = 0f;
            Alpha = 1f;
            DestroyLocalObject(toggleAnim);
        };

        AddLocalObject(toggleAnim);
        toggleAnim.Start();
    }

    public virtual SKRoundRect GetRect()
    {
        var rect = SKRect.Create((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        return new SKRoundRect(rect, roundRadius);
    }

    public int expandInteractionRect = 5;

    public virtual SKRoundRect GetInteractionRect()
    {
        var rect = SKRect.Create((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        var r = new SKRoundRect(rect, roundRadius);
        r.Deflate(-expandInteractionRect, -expandInteractionRect);
        return r;
    }

    ContextMenu? contextMenu = null;

    public virtual ContextMenu? CreateContextMenu() { return null; }
    public virtual ContextMenu? GetContextMenu() { return contextMenu; }
}