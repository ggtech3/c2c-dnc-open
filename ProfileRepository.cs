using System.Text.Json;

namespace WindowsDnc;

public sealed class ProfileRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string filePath;
    private readonly string legacyFilePath;

    public ProfileRepository(string? storagePath = null)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string directory = Path.Combine(localAppData, "C2CDncOpenPreview");
        filePath = storagePath ?? Path.Combine(directory, "profiles.json");
        // This review build never reads or overwrites another edition's profiles.
        legacyFilePath = filePath;
    }

    public IReadOnlyList<CncProfile> Load()
    {
        string loadPath = File.Exists(filePath) ? filePath : legacyFilePath;
        if (!File.Exists(loadPath))
        {
            return CncProfile.BuiltIn.ToList();
        }

        try
        {
            List<CncProfile>? stored = JsonSerializer.Deserialize<List<CncProfile>>(
                File.ReadAllText(loadPath),
                JsonOptions);

            if (stored is null || stored.Count == 0)
            {
                return CncProfile.BuiltIn.ToList();
            }

            // Saved profiles belong to the user. Removing a bundled starter must not
            // delete it or reset its settings, even if old builds stored IsBuiltIn=true.
            var profiles = stored.Select(item => item.IsBuiltIn &&
                    !CncProfile.BuiltIn.Any(b => b.Id == item.Id)
                ? item with { IsBuiltIn = false,
                    Guidance = "Legacy local profile - Not confirmed. " + item.Guidance }
                : item).ToList();
            foreach (var builtIn in CncProfile.BuiltIn)
                if (profiles.All(existing => existing.Id != builtIn.Id)) profiles.Add(builtIn);
            return profiles;
        }
        catch (JsonException)
        {
            return CncProfile.BuiltIn.ToList();
        }
        catch (IOException)
        {
            return CncProfile.BuiltIn.ToList();
        }
        catch (UnauthorizedAccessException)
        {
            return CncProfile.BuiltIn.ToList();
        }
    }

    public void Save(IEnumerable<CncProfile> profiles)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (directory is null)
        {
            throw new InvalidOperationException("Could not determine the profile storage directory.");
        }

        Directory.CreateDirectory(directory);
        string temporaryPath = filePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profiles, JsonOptions));
        File.Move(temporaryPath, filePath, true);
    }
}
