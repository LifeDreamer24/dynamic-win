using System.Windows.Forms;
using DynamicWin.Interop;
using DynamicWin.UI.Menu;
using DynamicWin.UI.Menu.Menus;

namespace DynamicWin.Shortcuts;

internal class MediaPreviousTrackShortcut(MenuManager menuManager) : IShortcut
{
    public bool Intended(Keys key, KeyModifier modifier)
    {
        return key is Keys.MediaPreviousTrack;
    }

    public void Execute()
    {
        if (menuManager.ActiveMenu is HomeMenu homeMenu)
            homeMenu.PrevSong();
    }
}