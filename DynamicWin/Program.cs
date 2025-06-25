using System.Windows;
using System.Windows.Forms;
using DynamicWin.UserSettings;

namespace DynamicWin;

public static class Program
{
    [STAThread]
    public static void Main() => new App(
        title: "DynamicWin Overlay",
        theme: new Theme(),
        style: WindowStyle.None,
        state: WindowState.Maximized,
        resizeMode: ResizeMode.NoResize,
        topmost: true,
        allowsTransparency: true,
        showInTaskbar: false,
        trayIcon: new NotifyIcon
        {
            Icon = new System.Drawing.Icon("Resources/icons/TrayIcon.ico"),
            Text = "DynamicWin",
        })
        .Run();
}