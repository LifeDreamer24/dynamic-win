using System.Windows.Forms;
using DynamicWin.Interop;
using DynamicWin.UI.UIElements;

namespace DynamicWin.Shortcuts;

internal class ToggleIslandShortcut(IslandObject islandObject) : IShortcut
{
    private readonly IslandObject _islandObject;
    
    public bool Intended(Keys key, KeyModifier modifier)
    {
        return key == Keys.LWin && modifier.IsCtrlDown;
    }

    public void Execute()
    {
        islandObject.Hidden = !islandObject.Hidden;
    }
}