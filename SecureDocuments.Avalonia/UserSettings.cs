using Newtonsoft.Json;
using SecureDocuments.Models;

namespace SecureDocuments.Avalonia;

public class UserSettings : IUserSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SecureDocuments", "settings.json");

    private SettingsData _data;

    public UserSettings()
    {
        _data = Load();
    }

    public string? ApplicationFolder
    {
        get => _data.ApplicationFolder;
        set { _data.ApplicationFolder = value; Save(); }
    }

    public string? LastUserEmail
    {
        get => _data.LastUserEmail;
        set { _data.LastUserEmail = value; Save(); }
    }

    public User? User { get; set; }

    public bool IsDarkTheme
    {
        get => _data.IsDarkTheme;
        set { _data.IsDarkTheme = value; Save(); }
    }

    public bool EnableNotification
    {
        get => _data.EnableNotification;
        set { _data.EnableNotification = value; Save(); }
    }

    private SettingsData Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonConvert.DeserializeObject<SettingsData>(json) ?? new SettingsData();
            }
        }
        catch { }
        return new SettingsData();
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(_data, Formatting.Indented));
        }
        catch { }
    }

    private sealed class SettingsData
    {
        public string? ApplicationFolder { get; set; }
        public string? LastUserEmail { get; set; }
        public bool IsDarkTheme { get; set; } = false;
        public bool EnableNotification { get; set; } = true;
    }
}
