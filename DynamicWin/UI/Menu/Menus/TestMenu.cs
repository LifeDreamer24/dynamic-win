using DynamicWin.UI.UIElements;
using DynamicWin.Utils;

namespace DynamicWin.UI.Menu.Menus;

public class TestMenu : BaseMenu
{
    public override List<UIObject> InitializeMenu(IslandObject island)
    {
        var objects = base.InitializeMenu(island);

        objects.Add(new DWText(island, "Test", Vec2.zero, UIAlignment.TopCenter));

        var btn = new DWTextButton(island, "Hello", new Vec2(0, 0), new Vec2(125, 25), () =>
        {
            MenuManager.OpenOverlayMenu(new TestMenu());

        }, UIAlignment.Center);

        objects.Add(btn);

        return objects;
    }
}