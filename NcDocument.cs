namespace WindowsDnc;

public sealed class NcDocument
{
    public string Text { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsDirty { get; set; }
    public bool IsReceived { get; set; }
    public int SelectionStart { get; set; }
    public int SelectionLength { get; set; }
    public int FirstVisibleLine { get; set; }
    public string FindText { get; set; } = string.Empty;
    public string ReplaceText { get; set; } = string.Empty;
    public bool MatchCase { get; set; }
    public bool WholeWord { get; set; }
    public bool ReplaceMode { get; set; }
    public bool HasKnownFileStamp { get; set; }
    public DateTime LastKnownWriteTimeUtc { get; set; }
    public long LastKnownFileLength { get; set; }
    public bool ExternalChangePromptOpen { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(FilePath)
        ? (IsReceived ? "Received program" : "Untitled")
        : Path.GetFileName(FilePath);

    public string TabTitle => DisplayName + (IsDirty ? " *" : string.Empty);
}
