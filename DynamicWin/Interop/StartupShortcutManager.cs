using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;

namespace DynamicWin.Interop;

public class StartupShortcutManager
{
    private static string GetStartupFolderPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    }

    private static string GetShortcutPath(string shortcutName)
    {
        return Path.Combine(GetStartupFolderPath(), $"{shortcutName}.lnk");
    }

    public static void TryCreateShortcut()
    {
        try
        {
            string appPath = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            string shortcutPath = GetShortcutPath(appPath);

            if (System.IO.File.Exists(shortcutPath))
            {
                Console.WriteLine("Shortcut already exists.");
                return;
            }

            string exePath = Process.GetCurrentProcess().MainModule.FileName;

            WshShell wshShell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.Description = "Launches the app on system startup.";
            shortcut.Save();

            Console.WriteLine("Shortcut created successfully.");
        }
        catch (Exception e)
        {
            
        }
    }

    public static void TryRemoveShortcut()
    {
        try
        {
            string appPath = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            string shortcutPath = GetShortcutPath(appPath);

            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
                Console.WriteLine("Shortcut removed successfully.");
                return;
            }

            Console.WriteLine("Shortcut does not exist.");
        }
        catch (Exception e)
        {
            
        }
    }
}