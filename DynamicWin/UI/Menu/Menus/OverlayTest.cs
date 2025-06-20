using DynamicWin.UI.UIElements;
using DynamicWin.Utils;

namespace DynamicWin.UI.Menu.Menus;

public class OverlayTest : BaseMenu
{
    public override List<UIObject> InitializeMenu(IslandObject island)
    {
        var objects = base.InitializeMenu(island);

        objects.Add(new DWText(island, "Overlay", Vec2.zero, UIAlignment.Center));

        return objects;
    }
}