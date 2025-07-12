using System.Windows.Forms;
using DynamicWin.Interop;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;
using DynamicWin.UserSettings;

namespace DynamicWin.Shortcuts;

internal class VolumeAdjustShortcut(
    PopupOptions.PopupOptionsSave popupOptions,
    MenuManager menuManager) : IShortcut
{
    public bool Intended(Keys key, KeyModifier modifier)
    {
        return key is Keys.VolumeDown or Keys.VolumeMute or Keys.VolumeUp 
               && popupOptions.volumePopup;
    }

    public void Execute()
    {
        if (menuManager.ActiveMenu is HomeMenu)
        {
            menuManager.OpenOverlay(new VolumeAdjustMenu(), 100f);
            return;
        }

        VolumeAdjustMenu.timerUntilClose = 0f;
    }
}