using System.IO;
using Newtonsoft.Json;

namespace DynamicWin.UserSettings;

internal static class SaveManager
{
    private static readonly Dictionary<string, object> _settingsDictionary = new();

    public static string SavePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "DynamicWin");
    
    private static string _fileName = "Settings.json";

    public static void LoadSettings()
    {
        System.Diagnostics.Debug.WriteLine(SavePath);

        Directory.CreateDirectory(SavePath);
        var fullPath = Path.Combine(SavePath, _fileName);

        if (!File.Exists(fullPath))
        {
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(new Dictionary<string, object>()));
            return;
        }

        var json = File.ReadAllText(fullPath);

        var deserializeSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
            ?? throw new InvalidOperationException("Failed to deserialize settings from JSON.");
        
        foreach (var setting in deserializeSettings)
            _settingsDictionary.TryAdd(setting.Key, setting.Value);
    }

    public static void SaveAll()
    {
        Directory.CreateDirectory(SavePath);

        var fullPath = Path.Combine(SavePath, _fileName);
        var json = JsonConvert.SerializeObject(_settingsDictionary, Formatting.Indented);

        File.WriteAllText(fullPath, json);
    }

    public static void AddOrUpdate(string key, object value)
    {
        if (Contains(key))
        {
            _settingsDictionary[key] = value;
            return;
        }
        
        _settingsDictionary.Add(key, value);
    }

    public static void Remove(string key)
    {
        if (Contains(key)) _settingsDictionary.Remove(key);
    }

    public static object? GetOrDefault(string key)
    {
        return Contains(key) ? _settingsDictionary[key] : null;
    }

    public static bool Contains(string key)
    {
        return _settingsDictionary.ContainsKey(key);
    }
}