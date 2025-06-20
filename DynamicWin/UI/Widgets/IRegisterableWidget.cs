using DynamicWin.Utils;

namespace DynamicWin.UI.Widgets;

public interface IRegisterableWidget
{
    public bool IsSmallWidget { get; }
    public string WidgetName { get; }
    public WidgetBase CreateWidgetInstance(UIObject? parent, Vec2 position, UIAlignment alignment = UIAlignment.TopCenter);
}