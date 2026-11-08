using System.Text.RegularExpressions;

namespace WindowsDnc;

public enum NcNavigationKind
{
    ToolChange,
    Operation,
    ProgramNumber,
    OptionalStop,
    ProgramEnd,
    Spindle,
    Coolant
}

public sealed record NcNavigationItem(
    NcNavigationKind Kind,
    string Label,
    int CharacterIndex,
    int LineNumber)
{
    public override string ToString() => $"{LineNumber,5}   {Label}";
}

public sealed class NcDocumentIndex
{
    private static readonly Regex ToolChangeRegex = new(
        @"(?:\bT\s*\d+\b.*\bM\s*0?6\b|\bM\s*0?6\b.*\bT\s*\d+\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CommentOnlyLineRegex = new(
        @"^\s*(?:\([^)]*\)\s*)+$",
        RegexOptions.Compiled);
    private static readonly Regex CommentRegex = new(
        @"\(([^)]*)\)",
        RegexOptions.Compiled);
    private static readonly Regex ToolDescriptionRegex = new(
        @"^T\s*\d+\b.*(?:D\s*=|CR\s*=|TAPER\s*=|ZMIN\s*=)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ProgramRegex = new(@"\bO\s*\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex StopRegex = new(@"\bM\s*0?[01]\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex EndRegex = new(@"\bM\s*(?:0?2|30)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SpindleRegex = new(@"(?:\bS\s*\d+(?:\.\d+)?|\bM\s*0?[345]\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex CoolantRegex = new(@"\bM\s*0?[789]\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<NcNavigationItem> Items { get; }

    private NcDocumentIndex(List<NcNavigationItem> items) => Items = items;

    public static NcDocumentIndex Build(string text)
    {
        var items = new List<NcNavigationItem>();
        int lineStart = 0;
        int lineNumber = 1;

        while (lineStart <= text.Length)
        {
            int lineEnd = text.IndexOf('\n', lineStart);
            if (lineEnd < 0)
            {
                lineEnd = text.Length;
            }

            string line = text[lineStart..lineEnd].TrimEnd('\r');
            AddFirstMatch(items, line, lineStart, lineNumber, ToolChangeRegex, NcNavigationKind.ToolChange, "Tool");
            AddFirstMatch(items, line, lineStart, lineNumber, ProgramRegex, NcNavigationKind.ProgramNumber, "Program");
            AddFirstMatch(items, line, lineStart, lineNumber, StopRegex, NcNavigationKind.OptionalStop, "Stop");
            AddFirstMatch(items, line, lineStart, lineNumber, EndRegex, NcNavigationKind.ProgramEnd, "End");
            AddFirstMatch(items, line, lineStart, lineNumber, SpindleRegex, NcNavigationKind.Spindle, "Spindle");
            AddFirstMatch(items, line, lineStart, lineNumber, CoolantRegex, NcNavigationKind.Coolant, "Coolant");

            if (CommentOnlyLineRegex.IsMatch(line))
            {
                foreach (Match comment in CommentRegex.Matches(line))
                {
                    string label = comment.Groups[1].Value.Trim();
                    if (label.Length > 0 && !ToolDescriptionRegex.IsMatch(label))
                    {
                        items.Add(new(
                            NcNavigationKind.Operation,
                            $"Operation  ({label})",
                            lineStart + comment.Index,
                            lineNumber));
                    }
                }
            }

            if (lineEnd == text.Length)
            {
                break;
            }
            lineStart = lineEnd + 1;
            lineNumber++;
        }

        return new NcDocumentIndex(items.OrderBy(item => item.CharacterIndex).ToList());
    }

    public NcNavigationItem? Find(
        NcNavigationKind kind,
        int currentPosition,
        bool forward)
    {
        List<NcNavigationItem> candidates = Items.Where(item => item.Kind == kind).ToList();
        if (candidates.Count == 0)
        {
            return null;
        }

        if (forward)
        {
            return candidates.FirstOrDefault(item => item.CharacterIndex > currentPosition) ?? candidates[0];
        }

        return candidates.LastOrDefault(item => item.CharacterIndex < currentPosition) ?? candidates[^1];
    }

    private static void AddFirstMatch(
        ICollection<NcNavigationItem> items,
        string line,
        int lineStart,
        int lineNumber,
        Regex expression,
        NcNavigationKind kind,
        string prefix)
    {
        Match match = expression.Match(line);
        if (match.Success)
        {
            items.Add(new(kind, $"{prefix}  {match.Value.Trim()}", lineStart + match.Index, lineNumber));
        }
    }
}
