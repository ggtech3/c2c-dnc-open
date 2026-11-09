namespace WindowsDnc;

/// <summary>Builds a reviewed sharing draft only. Never copies, opens a browser, or submits data.</summary>
public static class CommunitySharing
{
    public const string SubmissionUrl = "https://github.com/ggtech3/c2c-dnc-open/issues/new?title=Shared%20CNC%20profile";

    public static CommunityContribution CreateQuickContribution(CncProfile profile, string description,
        string? notes, string appVersion)
    {
        CommunityConfiguration.CheckSingleLine(description, "Machine/controller description", 120);
        if (notes is { Length: > 2000 })
            throw new FormatException("Notes must be at most 2000 characters.");
        string limitations = string.IsNullOrWhiteSpace(notes) ? "Not supplied" : notes;
        var context = new ContributionContext(
            Machine: description.Trim(),
            Controller: "Not supplied separately",
            FirmwareOptions: "Not supplied",
            MachineSideSettings: "Not supplied",
            AdapterDriver: "Not supplied",
            CableWiring: "Not supplied",
            ApplicationVersion: appVersion,
            TestDate: "",
            SendResult: "NotTested",
            ReceiveResult: "NotTested",
            DripFeedResult: "NotTested",
            Limitations: limitations,
            ContributorAlias: null);
        return CommunityConfiguration.CreateContribution(profile, description.Trim(), context);
    }

    public static string BuildGitHubPost(CommunityContribution contribution)
    {
        string json = CommunityConfiguration.Serialize(contribution);
        // Keep every user-controlled character inside one inert code block, even if the
        // serializer's escaping policy changes. No contained backtick run can close it.
        int longestRun = 0;
        int run = 0;
        foreach (char character in json)
        {
            run = character == '`' ? run + 1 : 0;
            longestRun = Math.Max(longestRun, run);
        }
        string fence = new('`', Math.Max(3, longestRun + 1));
        return "Shared CNC profile\n\n" +
            "Status: Not confirmed. This is a contributed setup, not a compatibility guarantee.\n" +
            "Only the selected profile's portable transfer settings and deliberately entered sharing details are included.\n" +
            "Missing details are stated explicitly. Structured test results are not inferred from notes.\n" +
            "Review the controller, cable and all settings before using a local copy.\n\n" +
            fence + "json\n" + json + "\n" + fence + "\n";
    }
}

public enum ShareDispatchResult { CopiedAndOpened, ClipboardFailed, BrowserFailed, AlreadyOpened, Busy }

/// <summary>Coordinates explicit sharing actions through injected callbacks; provides no OS implementation.</summary>
public sealed class ShareDispatch
{
    private readonly Action<string> copy;
    private readonly Action openBrowser;
    private int busy;
    private volatile bool hasOpened;

    public ShareDispatch(Action<string> copy, Action openBrowser)
    {
        this.copy = copy ?? throw new ArgumentNullException(nameof(copy));
        this.openBrowser = openBrowser ?? throw new ArgumentNullException(nameof(openBrowser));
    }

    public bool HasOpened => hasOpened;
    public string? LastError { get; private set; }

    public ShareDispatchResult TryCopyAndOpen(string post)
    {
        ArgumentNullException.ThrowIfNull(post);
        if (Interlocked.CompareExchange(ref busy, 1, 0) != 0) return ShareDispatchResult.Busy;
        try
        {
            if (hasOpened) return ShareDispatchResult.AlreadyOpened;
            LastError = null;
            try { copy(post); }
            catch (Exception ex) when (ex is System.Runtime.InteropServices.ExternalException or ThreadStateException)
            {
                LastError = ex.Message;
                return ShareDispatchResult.ClipboardFailed;
            }
            try { openBrowser(); }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or NotSupportedException)
            {
                LastError = ex.Message;
                return ShareDispatchResult.BrowserFailed;
            }
            hasOpened = true;
            return ShareDispatchResult.CopiedAndOpened;
        }
        finally { Interlocked.Exchange(ref busy, 0); }
    }
}
