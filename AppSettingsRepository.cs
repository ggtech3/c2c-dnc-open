using Microsoft.Win32;
using System.Text.Json;

namespace WindowsDnc;

public sealed record AppSettings(
    bool DarkMode,
    HashSet<string>? HiddenProfileIds = null,
    List<string>? RecentFiles = null,
    bool AlertsEnabled = true,
    string CompletionSound = "Laundry Celebration",
    string FailureSound = "Laundry Waltz",
    int AlertVolume = 70,
    int OutlineWidth = 300);

public sealed class AppSettingsRepository
{
    private readonly string filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "C2CDncOpenPreview",
        "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(filePath))
            {
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(filePath));
                if (settings is not null)
                {
                    return settings;
                }
            }
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or JsonException)
        {
            // Fall back to the Windows preference when settings cannot be read.
        }

        return new AppSettings(WindowsPrefersDarkMode());
    }

    public void Save(AppSettings settings)
    {
        string directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);
        string temporaryPath = filePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings));
        File.Move(temporaryPath, filePath, true);
    }

    private static bool WindowsPrefersDarkMode()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch
        {
            return false;
        }
    }
}
