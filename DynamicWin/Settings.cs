using DynamicWin.UI.Menu.Menus;
using DynamicWin.UI.UIElements;
using DynamicWin.Utils;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace DynamicWin.Main;

public class Settings
{
    private static IslandObject.IslandMode islandMode;
    private static bool allowBlur;
    private static bool allowAnimation;
    private static bool antiAliasing;
    private static bool runOnStartup;
    private static int theme;
    private static int activeScreenIndex;

    public static IslandObject.IslandMode IslandMode { get => islandMode; set => islandMode = value; }
    public static bool AllowBlur { get => allowBlur; set => allowBlur = value; }
    public static bool AllowAnimation { get => allowAnimation; set => allowAnimation = value; }
    public static bool AntiAliasing { get => antiAliasing; set => antiAliasing = value; }
    public static bool RunOnStartup { get => runOnStartup; set => runOnStartup = value; }
    public static int Theme { get => theme; set => theme = value; }
    public static int ScreenIndex { get => activeScreenIndex; set => activeScreenIndex = value; }

    public static List<string> smallWidgetsLeft;
    public static List<string> smallWidgetsRight;
    public static List<string> smallWidgetsMiddle;
    public static List<string> bigWidgets;

    public static void InitializeSettings()
    {
        try
        {

            if (SaveManager.Contains("settings"))
            {
                IslandMode = ((Int64)SaveManager.GetOrDefault("settings.islandmode") == 0) ? IslandObject.IslandMode.Island : IslandObject.IslandMode.Notch;

                AllowBlur = (bool)SaveManager.GetOrDefault("settings.allowblur");
                AllowAnimation = (bool)SaveManager.GetOrDefault("settings.allowanimtion");
                AntiAliasing = (bool)SaveManager.GetOrDefault("settings.antialiasing");
                RunOnStartup = (bool)SaveManager.GetOrDefault("settings.runonstartup");

                Theme = (int)((Int64)SaveManager.GetOrDefault("settings.theme"));
                ScreenIndex = (int)((Int64)SaveManager.GetOrDefault("settings.screenindex"));

                Settings.smallWidgetsLeft = new List<string>();
                Settings.smallWidgetsRight = new List<string>();
                Settings.smallWidgetsMiddle = new List<string>();
                Settings.bigWidgets = new List<string>();

                var smallWidgetsLeft = (JArray)SaveManager.GetOrDefault("settings.smallwidgetsleft");
                var smallWidgetsRight = (JArray)SaveManager.GetOrDefault("settings.smallwidgetsright");
                var smallWidgetsMiddle = (JArray)SaveManager.GetOrDefault("settings.smallwidgetsmiddle");
                var bigWidgets = (JArray)SaveManager.GetOrDefault("settings.bigwidgets");

                foreach (var x in smallWidgetsLeft)
                    Settings.smallWidgetsLeft.Add(x.ToString());
                foreach (var x in smallWidgetsRight)
                    Settings.smallWidgetsRight.Add(x.ToString());
                foreach (var x in smallWidgetsMiddle)
                    Settings.smallWidgetsMiddle.Add(x.ToString());
                foreach (var x in bigWidgets)
                    Settings.bigWidgets.Add(x.ToString());
            }
            else
            {
                smallWidgetsLeft = new List<string>();
                smallWidgetsRight = new List<string>();
                smallWidgetsMiddle = new List<string>();
                bigWidgets = new List<string>();

                smallWidgetsRight.Add("DynamicWin.UI.Widgets.Small.RegisterUsedDevicesWidget");
                smallWidgetsLeft.Add("DynamicWin.UI.Widgets.Small.RegisterTimeWidget");
                bigWidgets.Add("DynamicWin.UI.Widgets.Big.RegisterMediaWidget");

                IslandMode = IslandObject.IslandMode.Island;
                AllowBlur = true;
                AllowAnimation = true;
                AntiAliasing = true;

                Theme = 0;

                SaveManager.AddOrUpdate("settings", 1);
                SaveManager.SaveAll();
            }


            // This must be run after loading all settings
            AfterSettingsLoaded();
        }catch(Exception e)
        {
            MessageBox.Show("An error occured trying to load the settings. Please revert back to the default settings by deleting the \"Settings.json\" file located under \"%appdata%/DynamicWin/\".");

            smallWidgetsLeft = new List<string>();
            smallWidgetsRight = new List<string>();
            smallWidgetsMiddle = new List<string>();
            bigWidgets = new List<string>();

            AfterSettingsLoaded();
        }
    }

    static void AfterSettingsLoaded()
    {
        DynamicWin.Utils.Theme.Instance.UpdateTheme();

        var customOptions = SettingsMenu.LoadCustomOptions();

        foreach (var item in customOptions)
        {
            item.LoadSettings();
        }
    }

    public static void Save()
    {
        SaveManager.AddOrUpdate("settings.islandmode", (IslandMode == IslandObject.IslandMode.Island) ? 0 : 1);

        SaveManager.AddOrUpdate("settings.allowblur", AllowBlur);
        SaveManager.AddOrUpdate("settings.allowanimtion", AllowAnimation);
        SaveManager.AddOrUpdate("settings.antialiasing", AntiAliasing);
        SaveManager.AddOrUpdate("settings.runonstartup", RunOnStartup);

        SaveManager.AddOrUpdate("settings.theme", Theme);
        SaveManager.AddOrUpdate("settings.screenindex", ScreenIndex);

        SaveManager.AddOrUpdate("settings.smallwidgetsleft", smallWidgetsLeft);
        SaveManager.AddOrUpdate("settings.smallwidgetsright", smallWidgetsRight);
        SaveManager.AddOrUpdate("settings.smallwidgetsmiddle", smallWidgetsMiddle);
        SaveManager.AddOrUpdate("settings.bigwidgets", bigWidgets);

        SaveManager.SaveAll();
    }
}