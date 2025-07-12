using DynamicWin.Rendering;
using DynamicWin.Rendering.Primitives;
using DynamicWin.UI.UIElements;

namespace DynamicWin.UI.Menu;

public class BaseMenu : IDisposable
{
    public List<UIObject> Children { get; }

    public BaseMenu()
    {
        Children = [];
        InitializeMenu(DynamicWinRenderer.Instance.MainIsland);
        foreach (var child in Children)
        {
            //DynamicWinRenderer.Instance.MainIsland.AddChild(child);
        }
    }

    public virtual Vec2 IslandSize() { return new Vec2(200, 45); }
    public virtual Vec2 IslandSizeBig() { return IslandSize(); }

    public virtual Color IslandBorderColor() { return Color.Transparent; }

    public virtual List<UIObject> InitializeMenu(IslandObject island) { return Children; }

    public virtual void Update() { }

    public virtual void OnDeload() { }

    public void Dispose()
    {
        Children.Clear();
    }
}