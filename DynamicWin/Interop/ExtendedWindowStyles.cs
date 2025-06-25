using System.Windows;
using System.Windows.Interop;
using DynamicWin.Interop.DllImports;

// ReSharper disable InconsistentNaming

namespace DynamicWin.Interop;

internal static class ExtendedWindowStyles
{
    private const int GWL_EX_STYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APP_WINDOW = 0x00040000;
    
    internal static void ChangeToToolWindow(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle; // Define Handle
        var winStyle = User32.GetWindowLong(handle, GWL_EX_STYLE); // Fetch defined GWL_EXSTYLE

        // Apply WS_EX_TOOLWINDOW and remove WS_EX_APPWINDOW if it exists
        winStyle = (winStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APP_WINDOW;
        User32.SetWindowLong(handle, GWL_EX_STYLE, winStyle);
    }
}