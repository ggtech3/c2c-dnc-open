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

    public ProfileRepository()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string directory = Path.Combine(localAppData, "C2CDncOpenPreview");
        filePath = Path.Combine(directory, "profiles.json");
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

            var profiles = new List<CncProfile>();
            foreach (CncProfile builtIn in CncProfile.BuiltIn)
            {
                CncProfile selected = stored.FirstOrDefault(item => item.Id == builtIn.Id) ?? builtIn;
                // Upgrade the original EMCO preset once with the settings verified
                // against the user's known-working CIMCO configuration.
                if (selected.Id == "emco-et342-siemens-810t" && selected.CompatibilityRevision < 1)
                {
                    selected = selected with
                    {
                        ReceiveIdleTimeoutSeconds = 5,
                        OmitLinesContainingParenthesis = true,
                        RemoveReceiveControlCharacters = true,
                        CheckParity = true,
                        ParityReplaceCharacter = 35,
                        CompatibilityRevision = 1
                    };
                }
                profiles.Add(selected);
            }

            profiles.AddRange(stored.Where(item =>
                !item.IsBuiltIn &&
                profiles.All(existing => existing.Id != item.Id)));
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
