using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WindowsDnc;

public sealed record PortableTransferSettings(
    int BaudRate, int DataBits, Parity Parity, StopBits StopBits, Handshake Handshake,
    bool DtrEnable, bool RtsEnable, string NewLine, int LineDelayMilliseconds,
    bool ReceivePercentFramed, int? ReceiveIdleTimeoutSeconds, bool StripNulls,
    bool RemoveWhitespace, bool OmitParenthesisLines, bool OmitLinesContainingParenthesis,
    bool RemoveReceiveControlCharacters, bool CheckParity, int ParityReplaceCharacter);

public sealed record ContributionContext(
    string Machine, string Controller, string FirmwareOptions, string MachineSideSettings, string AdapterDriver,
    string CableWiring, string ApplicationVersion, string TestDate, string SendResult,
    string ReceiveResult, string DripFeedResult, string Limitations, string? ContributorAlias = null);

public sealed record CommunityContribution(
    int SchemaVersion, string Title, PortableTransferSettings Settings, ContributionContext Context);

/// <summary>Offline data boundary. Never reads local identity, opens a port, or persists a profile.</summary>
public static class CommunityConfiguration
{
    public const int CurrentSchemaVersion = 1;
    public const int MaximumFileBytes = 64 * 1024;
    public const int MaximumJsonDepth = 16;
    public const string SafetyCaution = "Historical setup only. Local copies can be edited and do not inherit confirmation. Match controller parameters, cable wiring and all settings before use. Software tests are not hardware validation.";
    internal static readonly JsonSerializerOptions JsonOptions = CreateOptions();
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectRequiredConstructorParameters = true,
            RespectNullableAnnotations = true,
            MaxDepth = MaximumJsonDepth,
            WriteIndented = true
        };
        options.Converters.Add(new ExactEnumConverter<Parity>());
        options.Converters.Add(new ExactEnumConverter<StopBits>());
        options.Converters.Add(new ExactEnumConverter<Handshake>());
        options.Converters.Add(new ExactEnumConverter<CommunityReviewStatus>());
        return options;
    }

    public static CommunityContribution CreateContribution(CncProfile profile, string title, ContributionContext context)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var settings = new PortableTransferSettings(profile.BaudRate, profile.DataBits, profile.Parity,
            profile.StopBits, profile.Handshake, profile.DtrEnable, profile.RtsEnable, profile.NewLine,
            profile.LineDelayMilliseconds, profile.ReceivePercentFramed, profile.ReceiveIdleTimeoutSeconds,
            profile.StripNulls, profile.RemoveWhitespace, profile.OmitParenthesisLines,
            profile.OmitLinesContainingParenthesis, profile.RemoveReceiveControlCharacters,
            profile.CheckParity, profile.ParityReplaceCharacter);
        var contribution = new CommunityContribution(CurrentSchemaVersion, title, settings, context);
        Validate(contribution);
        return contribution;
    }

    public static CommunityContribution Parse(string json)
    {
        var result = ParseDocument<CommunityContribution>(json);
        Validate(result);
        return result;
    }

    public static CommunityContribution ReadFile(string path)
    {
        // Read at most one byte beyond the limit, including when a file grows after opening.
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bytes = new byte[MaximumFileBytes + 1];
        int count = 0;
        while (count < bytes.Length)
        {
            int read = stream.Read(bytes, count, bytes.Length - count);
            if (read == 0) break;
            count += read;
        }
        if (count > MaximumFileBytes) throw new FormatException("Contribution exceeds the 64 KiB limit.");
        try
        {
            int offset = count >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf ? 3 : 0;
            return Parse(StrictUtf8.GetString(bytes, offset, count - offset));
        }
        catch (DecoderFallbackException ex) { throw new FormatException("Contribution must be valid UTF-8.", ex); }
    }

    public static string Serialize(CommunityContribution contribution)
    {
        Validate(contribution);
        string json = JsonSerializer.Serialize(contribution, JsonOptions);
        CheckSize(json);
        return json;
    }

    public static void Validate(CommunityContribution contribution)
    {
        if (contribution is null) throw new FormatException("Contribution is required.");
        if (contribution.SchemaVersion != CurrentSchemaVersion) throw new FormatException("Unsupported contribution schema version.");
        CheckSingleLine(contribution.Title, "Title", 120);
        var s = contribution.Settings ?? throw new FormatException("Settings are required.");
        CheckRange(s.BaudRate, 50, 921600, "Baud rate");
        CheckRange(s.DataBits, 5, 8, "Data bits");
        if (!Enum.IsDefined(s.Parity) || !Enum.IsDefined(s.Handshake) ||
            s.StopBits is not (StopBits.One or StopBits.OnePointFive or StopBits.Two))
            throw new FormatException("Invalid serial enumeration.");
        if (s.NewLine is not ("\r\n" or "\n" or "\r")) throw new FormatException("Line ending must be CR/LF, LF or CR.");
        CheckRange(s.LineDelayMilliseconds, 0, 60000, "Line delay");
        if (s.ReceiveIdleTimeoutSeconds is int idle) CheckRange(idle, 1, 3600, "Idle timeout");
        else if (!s.ReceivePercentFramed) throw new FormatException("Idle receive completion requires an idle timeout.");
        CheckRange(s.ParityReplaceCharacter, 0, 127, "Parity replacement");
        var c = contribution.Context ?? throw new FormatException("Contribution context is required.");
        CheckSingleLine(c.Machine, "Machine", 200);
        CheckSingleLine(c.Controller, "Controller", 200);
        CheckSingleLine(c.ApplicationVersion, "Application version", 200);
        foreach (string result in new[] { c.SendResult, c.ReceiveResult, c.DripFeedResult })
            if (result is not ("NotTested" or "Passed" or "Failed"))
                throw new FormatException("Test results must be NotTested, Passed or Failed.");
        if (c.TestDate != "" || c.SendResult != "NotTested" || c.ReceiveResult != "NotTested" || c.DripFeedResult != "NotTested")
            CheckDate(c.TestDate, "Test date");
        CheckText(c.FirmwareOptions, "Firmware/options", 2000);
        CheckText(c.MachineSideSettings, "Machine-side settings", 2000);
        CheckText(c.AdapterDriver, "Adapter/driver", 2000);
        CheckText(c.CableWiring, "Cable wiring", 2000);
        CheckText(c.SendResult, "Send result", 2000);
        CheckText(c.ReceiveResult, "Receive result", 2000);
        CheckText(c.DripFeedResult, "Drip-feed result", 2000);
        CheckText(c.Limitations, "Limitations", 2000);
        if (c.ContributorAlias is not null) CheckSingleLine(c.ContributorAlias, "Contributor alias", 120);
    }

    public static string BuildPreview(CommunityContribution contribution)
    {
        Validate(contribution);
        return "Not confirmed\r\nFile claims do not establish confirmation.\r\n\r\n" +
            SafetyCaution + "\r\n\r\n" + BuildDetails(contribution);
    }

    internal static string BuildDetails(CommunityContribution contribution)
    {
        Validate(contribution);
        var s = contribution.Settings;
        var c = contribution.Context;
        static string On(bool value) => value ? "On" : "Off";
        static string Field(string label, string? value) => label + ":\r\n  " +
            (value ?? "Not supplied").Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n  ") + "\r\n";
        string lineEnding = s.NewLine switch { "\r\n" => "CR/LF", "\n" => "LF", _ => "CR" };
        string details = $"Public title: {contribution.Title}\r\nSchema version: {contribution.SchemaVersion}\r\n\r\nTRANSFER SETTINGS\r\n" +
            $"Baud rate: {s.BaudRate}\r\nData bits: {s.DataBits}\r\nParity: {s.Parity}\r\nStop bits: {s.StopBits}\r\nFlow control: {s.Handshake}\r\n" +
            $"DTR: {On(s.DtrEnable)}\r\nRTS: {On(s.RtsEnable)}\r\nLine ending: {lineEnding}\r\nLine delay: {s.LineDelayMilliseconds} ms\r\n" +
            $"Receive completion: {(s.ReceivePercentFramed ? "Starts/ends with %" : "Idle timeout")}\r\n" +
            $"Receive idle timeout/fallback: {(s.ReceiveIdleTimeoutSeconds is int idle ? idle + " seconds" : "None")}\r\n" +
            $"Strip NULs: {On(s.StripNulls)}\r\nRemove spaces/tabs: {On(s.RemoveWhitespace)}\r\n" +
            $"Omit send lines beginning with (: {On(s.OmitParenthesisLines)}\r\nOmit send lines containing (: {On(s.OmitLinesContainingParenthesis)}\r\n" +
            $"Remove received control characters: {On(s.RemoveReceiveControlCharacters)}\r\nReplace parity-error characters: {On(s.CheckParity)}\r\nParity replacement: ASCII {s.ParityReplaceCharacter}\r\n\r\nCONTRIBUTION CONTEXT\r\n";
        details += Field("Contributor alias", c.ContributorAlias) + Field("Machine", c.Machine) + Field("Controller", c.Controller) +
            Field("Firmware/options", c.FirmwareOptions) + Field("Machine-side settings", c.MachineSideSettings) +
            Field("Adapter/driver", c.AdapterDriver) + Field("Cable wiring", c.CableWiring) + Field("Application version", c.ApplicationVersion) +
            Field("Test date", c.TestDate.Length == 0 ? "Not tested" : c.TestDate) + Field("Send result", c.SendResult) +
            Field("Receive result", c.ReceiveResult) + Field("Drip-feed result", c.DripFeedResult) + Field("Limitations", c.Limitations);
        return details + "\r\nEXACT CONTRIBUTION JSON\r\n" + Serialize(contribution);
    }
    public static CncProfile CreateLocalProfile(CommunityContribution contribution, string localName, string localPort)
        => CreateLocalCopy(contribution, localName, localPort, "Imported from a contribution file. Original status: Not confirmed.");

    /// <summary>Save a candidate snapshot before the caller replaces its in-memory list.</summary>
    public static List<CncProfile> SaveNewProfile(IReadOnlyList<CncProfile> existing, CncProfile imported,
        Action<IEnumerable<CncProfile>> save)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(imported);
        ArgumentNullException.ThrowIfNull(save);
        if (imported.IsBuiltIn || string.IsNullOrWhiteSpace(imported.Id) ||
            existing.Any(p => string.Equals(p.Id, imported.Id, StringComparison.OrdinalIgnoreCase)))
            throw new FormatException("An imported profile must have a new custom identity.");
        CheckSingleLine(imported.Name, "Local profile name", 120);
        if (existing.Any(p => string.Equals(p.Name.Trim(), imported.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new FormatException("Choose a local profile name that is not already used.");
        var candidate = existing.Append(imported).ToList();
        save(candidate.AsReadOnly());
        return candidate;
    }
    internal static CncProfile CreateLocalCopy(CommunityContribution contribution, string localName, string localPort, string provenance)
    {
        Validate(contribution);
        CheckSingleLine(localName, "Local profile name", 120);
        CheckSingleLine(localPort, "Local COM port", 16);
        string port = localPort.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(port, @"\ACOM[1-9][0-9]{0,3}\z", RegexOptions.CultureInvariant))
            throw new FormatException("Enter a local COM port such as COM1 (COM1 through COM9999).");
        var s = contribution.Settings;
        return new CncProfile(Guid.NewGuid().ToString("N"), false, localName.Trim(), port,
            s.BaudRate, s.DataBits, s.Parity, s.StopBits, s.Handshake, s.DtrEnable, s.RtsEnable,
            s.NewLine, s.LineDelayMilliseconds, s.ReceivePercentFramed, s.ReceiveIdleTimeoutSeconds,
            s.StripNulls, s.RemoveWhitespace, s.OmitParenthesisLines,
            $"Historical contribution: {contribution.Title}. {provenance} {SafetyCaution}",
            "", s.OmitLinesContainingParenthesis, s.RemoveReceiveControlCharacters, s.CheckParity,
            s.ParityReplaceCharacter, 0);
    }

    internal static T ParseDocument<T>(string json) where T : class
    {
        CheckSize(json);
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = MaximumJsonDepth });
            CheckDuplicates(document.RootElement);
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new FormatException("JSON object is required.");
        }
        catch (JsonException ex) { throw new FormatException("Invalid configuration JSON: " + ex.Message, ex); }
    }

    private static void CheckSize(string json)
    {
        if (json is null) throw new FormatException("JSON is required.");
        try
        {
            if (json.Length > MaximumFileBytes || StrictUtf8.GetByteCount(json) > MaximumFileBytes)
                throw new FormatException("Configuration exceeds the 64 KiB limit.");
        }
        catch (EncoderFallbackException ex) { throw new FormatException("Invalid Unicode text.", ex); }
    }

    private static void CheckDuplicates(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new FormatException("Duplicate JSON member: " + property.Name);
                CheckDuplicates(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) CheckDuplicates(item);
    }

    internal static void CheckText(string? value, string label, int maximum)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximum)
            throw new FormatException($"{label} is required and must be at most {maximum} characters.");
        if (value.Any(c => char.IsControl(c) && c is not ('\r' or '\n' or '\t')) ||
            value.Any(c => c is >= '\u202a' and <= '\u202e' or >= '\u2066' and <= '\u2069'))
            throw new FormatException(label + " contains unsupported control characters.");
    }

    internal static void CheckSingleLine(string? value, string label, int maximum)
    {
        CheckText(value, label, maximum);
        if (value!.Any(c => c is '\r' or '\n' or '\t' or '\u2028' or '\u2029'))
            throw new FormatException(label + " must be a single line.");
    }
    internal static void CheckDate(string? value, string label)
    {
        if (!DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            throw new FormatException(label + " must be a valid yyyy-MM-dd date.");
    }

    private static void CheckRange(int value, int min, int max, string label)
    {
        if (value < min || value > max) throw new FormatException($"{label} must be between {min} and {max}.");
    }

    private sealed class ExactEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String) throw new JsonException("Enum values must be named strings.");
            string? name = reader.GetString();
            if (name is null || !Enum.GetNames<T>().Contains(name, StringComparer.Ordinal) || !Enum.TryParse<T>(name, out var result))
                throw new JsonException("Invalid " + typeof(T).Name + " value.");
            return result;
        }
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
}
