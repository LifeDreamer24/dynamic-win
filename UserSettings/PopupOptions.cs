using DynamicWin.Rendering.Primitives;
using DynamicWin.UI;
using DynamicWin.UI.Menu.Menus;
using DynamicWin.UI.Widgets;
using Newtonsoft.Json;

namespace DynamicWin.UserSettings;

internal class PopupOptions : IRegisterableSetting
{
    public string SettingID => "popup_options";

    public string SettingTitle => "Pop-up Options";

    public static PopupOptionsSave SaveData;

    public struct PopupOptionsSave
    {
        public bool volumePopup;
        public bool brightnessPopup;
    }

    public void LoadSettings()
    {
        if (SaveManager.Contains(SettingID))
        {
            SaveData = JsonConvert.DeserializeObject<PopupOptionsSave>((string)SaveManager.GetOrDefault(SettingID));
        }
        else
        {
            SaveData = new PopupOptionsSave() { volumePopup = true, brightnessPopup = true };
        }
    }

    public void SaveSettings()
    {
        SaveManager.AddOrUpdate(SettingID, JsonConvert.SerializeObject(SaveData));
    }

    public List<UIObject> SettingsObjects()
    {
        var objects = new List<UIObject>();

        var volume = new Checkbox(null, "Display volume pop-up", new Vec2(25, 0), new Vec2(25, 25), null, UIAlignment.TopLeft);

        volume.clickCallback += () =>
        {
            SaveData.volumePopup = volume.IsChecked;
        };

        volume.IsChecked = SaveData.volumePopup;
        volume.Anchor.X = 0;
        objects.Add(volume);

        var brightness = new Checkbox(null, "Display brightness pop-up", new Vec2(25, 0), new Vec2(25, 25), null, UIAlignment.TopLeft);

        brightness.clickCallback += () =>
        {
            SaveData.brightnessPopup = volume.IsChecked;
        };

        brightness.IsChecked = SaveData.brightnessPopup;
        brightness.Anchor.X = 0;
        objects.Add(brightness);

        return objects;
    }
}