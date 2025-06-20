﻿using DynamicWin.Main;
using DynamicWin.UI.UIElements;
using DynamicWin.Utils;

namespace DynamicWin.UI.Menu;

public class BaseMenu : IDisposable
{
    private List<UIObject> uiObjects;

    public List<UIObject> UiObjects => uiObjects;

    public BaseMenu()
    {
        uiObjects = InitializeMenu(RendererMain.Instance.MainIsland);
    }

    public virtual Vec2 IslandSize() { return new Vec2(200, 45); }
    public virtual Vec2 IslandSizeBig() { return IslandSize(); }

    public virtual Color IslandBorderColor() { return Color.Transparent; }

    public virtual List<UIObject> InitializeMenu(IslandObject island) { return new List<UIObject>(); }

    public virtual void Update() { }

    public virtual void OnDeload() { }

    public void Dispose()
    {
        uiObjects.Clear();
    }
}