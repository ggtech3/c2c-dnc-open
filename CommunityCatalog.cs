using System.Text;
using System.Text.RegularExpressions;

namespace WindowsDnc;

public enum CommunityReviewStatus { UserReported, IndependentlyConfirmed }
public sealed record CommunityConfirmation(string Tester, string TestDate, string Evidence,
    string SendResult, string ReceiveResult, string DripFeedResult);
public sealed record CommunityCatalogEntry(string Id, int Revision, CommunityContribution Contribution,
    CommunityReviewStatus Status, string Reviewer, string ReviewDate, string ReviewNotes,
    CommunityConfirmation? Confirmation);
public sealed record CommunityCatalogDocument(int SchemaVersion, IReadOnlyList<CommunityCatalogEntry> Entries);

/// <summary>Trusted review labels originate only from the catalog embedded in this build.</summary>
public static class CommunityCatalog
{
    public const string ResourceName = "C2CDnc.CommunityConfigurations.json";

    public static IReadOnlyList<CommunityCatalogEntry> LoadEmbedded()
    {
        using var stream = typeof(CommunityCatalog).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new FormatException("The embedded community catalog is missing.");
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: false);
        // The catalog is bounded too, even if an incorrectly packaged build changes the resource.
        var chars = new char[CommunityConfiguration.MaximumFileBytes + 1];
        try
        {
            int count = reader.ReadBlock(chars, 0, chars.Length);
            string json = new string(chars, 0, count);
            return Parse(json.StartsWith('\uFEFF') ? json[1..] : json);
        }
        catch (DecoderFallbackException ex) { throw new FormatException("Catalog must be valid UTF-8.", ex); }
    }

    /// <summary>Validates curated build/test input. Never use this API to grant trust to an imported file.</summary>
    public static IReadOnlyList<CommunityCatalogEntry> Parse(string json)
    {
        var document = CommunityConfiguration.ParseDocument<CommunityCatalogDocument>(json);
        if (document.SchemaVersion != CommunityConfiguration.CurrentSchemaVersion)
            throw new FormatException("Unsupported catalog schema version.");
        if (document.Entries is null) throw new FormatException("Catalog entries are required.");
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in document.Entries)
        {
            ValidateEntry(entry);
            if (!ids.Add(entry.Id)) throw new FormatException("Duplicate catalog ID: " + entry.Id);
        }
        return Array.AsReadOnly(document.Entries.ToArray());
    }

    public static void ValidateEntry(CommunityCatalogEntry entry)
    {
        if (entry is null) throw new FormatException("Catalog entry cannot be null.");
        CommunityConfiguration.CheckText(entry.Id, "Catalog ID", 80);
        if (!Regex.IsMatch(entry.Id, @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z", RegexOptions.CultureInvariant))
            throw new FormatException("Catalog ID must be stable lowercase letters, numbers and separated hyphens.");
        if (entry.Revision < 1) throw new FormatException("Catalog revision must be positive.");
        CommunityConfiguration.Validate(entry.Contribution);
        if (!Enum.IsDefined(entry.Status)) throw new FormatException("Invalid catalog review status.");
        CommunityConfiguration.CheckSingleLine(entry.Reviewer, "Reviewer", 120);
        CommunityConfiguration.CheckDate(entry.ReviewDate, "Review date");
        CommunityConfiguration.CheckText(entry.ReviewNotes, "Review notes", 2000);
        if (entry.Status == CommunityReviewStatus.UserReported)
        {
            if (entry.Confirmation is not null) throw new FormatException("User-reported entries must not include independent confirmation.");
            return;
        }
        var confirmation = entry.Confirmation ?? throw new FormatException("Independent confirmation requires evidence.");
        CommunityConfiguration.CheckSingleLine(confirmation.Tester, "Independent tester", 120);
        CommunityConfiguration.CheckDate(confirmation.TestDate, "Confirmation date");
        CommunityConfiguration.CheckText(confirmation.Evidence, "Independent test evidence", 2000);
        string? contributor = entry.Contribution.Context.ContributorAlias;
        CommunityConfiguration.CheckSingleLine(contributor, "Contributor alias for independent confirmation", 120);
        if (string.Equals(contributor!.Trim(), confirmation.Tester.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new FormatException("The confirming tester must be separate from the contributor.");
        foreach (string result in new[] { confirmation.SendResult, confirmation.ReceiveResult, confirmation.DripFeedResult })
            if (result is not ("NotTested" or "Passed" or "Failed"))
                throw new FormatException("Independent test results must be NotTested, Passed or Failed.");
        var original = entry.Contribution.Context;
        if (!((confirmation.SendResult == "Passed" && original.SendResult == "Passed") ||
              (confirmation.ReceiveResult == "Passed" && original.ReceiveResult == "Passed") ||
              (confirmation.DripFeedResult == "Passed" && original.DripFeedResult == "Passed")))
            throw new FormatException("Independent confirmation requires at least one operation reported Passed by both the contributor and the separate tester.");
        if (string.CompareOrdinal(entry.ReviewDate, confirmation.TestDate) < 0)
            throw new FormatException("Review date cannot precede independent testing.");
    }

    public static string GetConfirmationLabel(CommunityCatalogEntry entry)
    {
        ValidateEntry(entry);
        return entry.Status == CommunityReviewStatus.IndependentlyConfirmed ? "Confirmed" : "Not confirmed";
    }
    public static string BuildPreview(CommunityCatalogEntry entry)
    {
        ValidateEntry(entry);
        string status = GetConfirmationLabel(entry) + (entry.Status == CommunityReviewStatus.IndependentlyConfirmed
            ? "\r\nConfirmation covers only the original setup and operations marked Passed by both the contributor and separate tester below. Failed and untested operations are not confirmed."
            : "\r\nMaintainer data review does not establish confirmation.");
        string confirmation = entry.Confirmation is { } c
            ? $"\r\nSeparate tester: {c.Tester}\r\nConfirmation date: {c.TestDate}\r\nIndependent evidence: {c.Evidence}\r\nIndependent send: {c.SendResult}\r\nIndependent receive: {c.ReceiveResult}\r\nIndependent drip-feed: {c.DripFeedResult}" : "";
        return $"{status}\r\nCatalog ID: {entry.Id}\r\nRevision: {entry.Revision}\r\nReviewer: {entry.Reviewer}\r\nReview date: {entry.ReviewDate}\r\nReview notes: {entry.ReviewNotes}{confirmation}\r\n\r\n" +
            CommunityConfiguration.SafetyCaution + "\r\n\r\n" + CommunityConfiguration.BuildDetails(entry.Contribution);
    }

    public static CncProfile CreateLocalProfile(CommunityCatalogEntry entry, string localName, string localPort)
    {
        ValidateEntry(entry);
        return CommunityConfiguration.CreateLocalCopy(entry.Contribution, localName, localPort,
            $"Historical catalog source: {entry.Id}, revision {entry.Revision}; original status: {GetConfirmationLabel(entry)} on {entry.ReviewDate}.");
    }
}
