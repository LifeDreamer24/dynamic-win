using DynamicWin.Rendering.Primitives;
using DynamicWin.UserSettings;

namespace DynamicWin.UI.UIElements;

public class DWButton : UIObject
{
    // Button Color

    public Color normalColor = Theme.IconColor.Override(alpha: 0.15f);
    public Color hoverColor = Theme.IconColor.Override(alpha: 0.25f);
    public Color clickColor = Theme.IconColor.Override(alpha: 0.65f);

    public float colorSmoothingSpeed = 15f;

    // Button Size

    protected Vec2 initialScale;

    public Vec2 normalScaleMulti = Vec2.one * 1f;
    public Vec2 hoverScaleMulti = Vec2.one * 1.05f;
    public Vec2 clickScaleMulti = Vec2.one * 1f - 0.05f;

    protected Vec2 scaleMultiplier = Vec2.one;

    public SecondOrder scaleSecondOrder;

    // Events

    public Action clickCallback;

    public DWButton(UIObject? parent, Vec2 position, Vec2 size, Action clickCallback, UIAlignment alignment = UIAlignment.TopCenter) : base(parent, position, size, alignment)
    {
        initialScale = size;

        roundRadius = 5f;
        scaleSecondOrder = new SecondOrder(size, 4.5f, 0.45f, 0.15f);
        this.clickCallback = clickCallback;

        Color = normalColor;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        Vec2 currentSize = initialScale;
        scaleMultiplier = Vec2.one;

        if (IsHovering && !IsMouseDown)
            scaleMultiplier *= hoverScaleMulti;
        else if (IsMouseDown)
            scaleMultiplier *= clickScaleMulti;
        else if (!IsHovering && !IsMouseDown)
            scaleMultiplier *= normalScaleMulti;
        else
            scaleMultiplier *= normalScaleMulti;

        currentSize *= scaleMultiplier;

        Size = scaleSecondOrder.Update(deltaTime, currentSize);

        if (IsHovering && !IsMouseDown)
            Color = GetColor(Color.LinearInterpolation(Color, hoverColor, colorSmoothingSpeed * deltaTime));
        else if (IsMouseDown)
            Color = GetColor(Color.LinearInterpolation(Color, clickColor, colorSmoothingSpeed * deltaTime));
        else if (!IsHovering && !IsMouseDown)
            Color = GetColor(Color.LinearInterpolation(Color, normalColor, colorSmoothingSpeed * deltaTime));
        else
            Color = GetColor(Color.LinearInterpolation(Color, normalColor, colorSmoothingSpeed * deltaTime));
    }

    public override void OnMouseUp()
    {
        clickCallback?.Invoke();
    }
}