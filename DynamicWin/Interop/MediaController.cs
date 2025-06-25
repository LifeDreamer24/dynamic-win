using DynamicWin.Interop.DllImports;

// ReSharper disable InconsistentNaming

namespace DynamicWin.Interop;

public class MediaController
{
    private const byte VK_MEDIA_PLAY_PAUSE = 0xB3;
    private const byte VK_MEDIA_NEXT_TRACK = 0xB0;
    private const byte VK_MEDIA_PREV_TRACK = 0xB1;

    public void PlayPause()
    {
        User32.Keybd_event(VK_MEDIA_PLAY_PAUSE, 0, 0, 0);
    }

    public void Next()
    {
        User32.Keybd_event(VK_MEDIA_NEXT_TRACK, 0, 0, 0);
    }

    public void Previous()
    {
        User32.Keybd_event(VK_MEDIA_PREV_TRACK, 0, 0, 0);
    }
}