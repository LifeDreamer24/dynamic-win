using DynamicWin.Resources;
using System.Windows;
using System.Windows.Forms;
using DynamicWin.Interop;
using DynamicWin.Rendering;
using DynamicWin.Shortcuts;
using DynamicWin.UI.Menu;
using DynamicWin.UserSettings;
using MessageBox = System.Windows.MessageBox;

namespace DynamicWin;

public class App(
    string title,
    WindowStyle style,
    WindowState state,
    ResizeMode resizeMode,
    bool topmost,
    bool allowsTransparency,
    bool showInTaskbar,
    Theme theme,
    NotifyIcon trayIcon) : Application
{
    private Mutex? _mutex;
    
    public static string Version => "1.1.0b";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = new HardwareMonitor();
        
        var anotherInstanceRunning = CheckIfAnotherInstanceIsRunning();
        if (anotherInstanceRunning)
        {
            var errorForm = new ErrorForm();
            errorForm.Show();
            return;
        }
        
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Dispatcher.UnhandledException += Dispatcher_UnhandledException;

        Theme.InitTheme();
        SaveManager.LoadSettings();
        FileResources.LoadResources();
        KeyboardListener.Start();
        Settings.InitializeSettings();
        
        var renderer = new DynamicWinRenderer();
        
        var mainForm = new MainForm(
            title, style, state, resizeMode, topmost, 
            allowsTransparency, showInTaskbar, theme, trayIcon,
            renderer: renderer,
            shortcuts: [
                new ToggleIslandShortcut(DynamicWinRenderer.Instance.MainIsland),
                new VolumeAdjustShortcut(PopupOptions.SaveData, MenuManager.Instance),
                new MediaPreviousTrackShortcut(MenuManager.Instance),
                new MediaNextTrackShortcut(MenuManager.Instance),
            ]);
        
        mainForm.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        SaveManager.SaveAll();
        HardwareMonitor.Stop();

        MainForm.Instance.DisposeTrayIcon();

        KeyboardListener.Stop();
        GC.KeepAlive(_mutex); // Important
    }

    private bool CheckIfAnotherInstanceIsRunning()
    {
        _mutex = new Mutex(true, "FlorianButz.DynamicWin", out var result);
        return !result;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Unhandled exception: {e.ExceptionObject}");
    }

    private static void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Unhandled exception: {e.Exception}");
        e.Handled = true; // Prevent the application from terminating
    }
}