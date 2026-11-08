using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace WindowsDnc;

public static partial class GCodeSyntaxHighlighter
{
    private const int WmSetRedraw = 0x000B;
    private const int EmGetFirstVisibleLine = 0x00CE;
    private const int EmLineScroll = 0x00B6;
    private const int EmSetSelection = 0x00B1;
    private const int EmSetCharacterFormat = 0x0444;
    private const int ScfSelection = 0x0001;
    private const uint CfmColor = 0x40000000;
    private const int BufferLinesBefore = 8;
    private const int BufferLinesAfter = 16;
    private const int MaximumHighlightedDocumentCharacters = 1_000_000;

    public static void WarmUp()
    {
        const string sample = "N10 G01 M03 X1 Y2 Z3 A4 B5 C6 F20 H1 (COMMENT) %";
        _ = AddressWordRegex().IsMatch(sample);
        _ = XAxisRegex().IsMatch(sample);
        _ = YAxisRegex().IsMatch(sample);
        _ = ZAxisRegex().IsMatch(sample);
        _ = AAxisRegex().IsMatch(sample);
        _ = BAxisRegex().IsMatch(sample);
        _ = CAxisRegex().IsMatch(sample);
        _ = FeedRegex().IsMatch(sample);
        _ = GCodeRegex().IsMatch(sample);
        _ = MCodeRegex().IsMatch(sample);
        _ = SequenceRegex().IsMatch(sample);
        _ = PercentRegex().IsMatch(sample);
        _ = CommentRegex().IsMatch(sample);
    }

    public static void Apply(RichTextBox editor, bool dark)
    {
        if (editor.TextLength == 0 ||
            editor.TextLength > MaximumHighlightedDocumentCharacters ||
            editor.IsDisposed)
        {
            return;
        }

        (int rangeStart, int rangeLength) = GetHighlightRange(editor);
        if (rangeLength <= 0)
        {
            return;
        }

        int selectionStart = editor.SelectionStart;
        int selectionLength = editor.SelectionLength;
        int firstVisibleLine = (int)SendMessage(editor.Handle, EmGetFirstVisibleLine, 0, 0);

        SendMessage(editor.Handle, WmSetRedraw, 0, 0);
        try
        {
            editor.Select(rangeStart, rangeLength);
            string visibleText = editor.SelectedText;
            SetTextColor(editor, rangeStart, rangeLength, ThemeManager.Foreground(dark));

            ApplyMatches(editor, visibleText, rangeStart, AddressWordRegex(), dark
                ? Color.FromArgb(125, 184, 213)
                : Color.FromArgb(55, 82, 96));
            ApplyMatches(editor, visibleText, rangeStart, XAxisRegex(), dark
                ? Color.FromArgb(255, 103, 97)
                : Color.FromArgb(190, 38, 38));
            ApplyMatches(editor, visibleText, rangeStart, YAxisRegex(), dark
                ? Color.FromArgb(57, 211, 83)
                : Color.FromArgb(20, 128, 48));
            ApplyMatches(editor, visibleText, rangeStart, ZAxisRegex(), dark
                ? Color.FromArgb(158, 143, 255)
                : Color.FromArgb(84, 67, 174));
            ApplyMatches(editor, visibleText, rangeStart, AAxisRegex(), dark
                ? Color.FromArgb(50, 214, 197)
                : Color.FromArgb(0, 118, 112));
            ApplyMatches(editor, visibleText, rangeStart, BAxisRegex(), dark
                ? Color.FromArgb(255, 158, 100)
                : Color.FromArgb(174, 77, 0));
            ApplyMatches(editor, visibleText, rangeStart, CAxisRegex(), dark
                ? Color.FromArgb(240, 113, 196)
                : Color.FromArgb(155, 28, 111));
            ApplyMatches(editor, visibleText, rangeStart, FeedRegex(), dark
                ? Color.FromArgb(255, 218, 72)
                : Color.FromArgb(145, 98, 0));
            ApplyMatches(editor, visibleText, rangeStart, GCodeRegex(), dark
                ? Color.FromArgb(74, 163, 255)
                : Color.FromArgb(0, 85, 166));
            ApplyMatches(editor, visibleText, rangeStart, MCodeRegex(), dark
                ? Color.FromArgb(204, 139, 255)
                : Color.FromArgb(112, 44, 153));
            ApplyMatches(editor, visibleText, rangeStart, SequenceRegex(), dark
                ? Color.FromArgb(0, 183, 255)
                : Color.FromArgb(0, 103, 153));
            ApplyMatches(editor, visibleText, rangeStart, PercentRegex(), dark
                ? Color.FromArgb(255, 122, 162)
                : Color.FromArgb(190, 35, 89));
            ApplyMatches(editor, visibleText, rangeStart, CommentRegex(), dark
                ? Color.FromArgb(113, 199, 167)
                : Color.FromArgb(36, 122, 98));

            editor.Select(
                Math.Min(selectionStart, editor.TextLength),
                Math.Min(selectionLength, Math.Max(0, editor.TextLength - selectionStart)));

            int currentFirstLine = (int)SendMessage(editor.Handle, EmGetFirstVisibleLine, 0, 0);
            SendMessage(editor.Handle, EmLineScroll, 0, firstVisibleLine - currentFirstLine);
        }
        finally
        {
            SendMessage(editor.Handle, WmSetRedraw, 1, 0);
            editor.Invalidate();
        }
    }

    private static (int Start, int Length) GetHighlightRange(RichTextBox editor)
    {
        int firstVisibleCharacter = editor.GetCharIndexFromPosition(new Point(1, 1));
        int lastVisibleCharacter = editor.GetCharIndexFromPosition(
            new Point(Math.Max(1, editor.ClientSize.Width - 2), Math.Max(1, editor.ClientSize.Height - 2)));

        int firstVisibleLine = editor.GetLineFromCharIndex(firstVisibleCharacter);
        int lastVisibleLine = editor.GetLineFromCharIndex(lastVisibleCharacter);
        int firstLine = Math.Max(0, firstVisibleLine - BufferLinesBefore);
        int lastLine = lastVisibleLine + BufferLinesAfter;

        int start = editor.GetFirstCharIndexFromLine(firstLine);
        if (start < 0)
        {
            start = 0;
        }

        int end = editor.GetFirstCharIndexFromLine(lastLine + 1);
        if (end < start)
        {
            end = editor.TextLength;
        }

        return (start, end - start);
    }

    private static void ApplyMatches(
        RichTextBox editor,
        string text,
        int offset,
        Regex expression,
        Color color)
    {
        foreach (Match match in expression.Matches(text))
        {
            SetTextColor(editor, offset + match.Index, match.Length, color);
        }
    }

    private static void SetTextColor(RichTextBox editor, int start, int length, Color color)
    {
        SendMessage(editor.Handle, EmSetSelection, start, start + length);
        var format = new CharacterFormat
        {
            Size = (uint)Marshal.SizeOf<CharacterFormat>(),
            Mask = CfmColor,
            TextColor = color.R | (color.G << 8) | (color.B << 16),
            FaceName = string.Empty
        };
        SendMessage(editor.Handle, EmSetCharacterFormat, ScfSelection, ref format);
    }

    [GeneratedRegex(@"(?<![A-Z0-9])[DEFIJKLPQRSTUVWH]\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex AddressWordRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])X\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex XAxisRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])Y\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex YAxisRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])Z\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex ZAxisRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])A\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex AAxisRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])B\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex BAxisRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])C\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex CAxisRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])F\s*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex FeedRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])G\s*\d+(?:\.\d+)?", RegexOptions.IgnoreCase)]
    private static partial Regex GCodeRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])M\s*\d+(?:\.\d+)?", RegexOptions.IgnoreCase)]
    private static partial Regex MCodeRegex();

    [GeneratedRegex(@"(?<![A-Z0-9])[NO]\s*\d+", RegexOptions.IgnoreCase)]
    private static partial Regex SequenceRegex();

    [GeneratedRegex(@"%")]
    private static partial Regex PercentRegex();

    [GeneratedRegex(@"\([^\r\n)]*\)|;[^\r\n]*")]
    private static partial Regex CommentRegex();

    [DllImport("user32.dll")]
    private static extern nint SendMessage(nint window, int message, nint wParam, nint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(
        nint window,
        int message,
        nint wParam,
        ref CharacterFormat lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CharacterFormat
    {
        public uint Size;
        public uint Mask;
        public uint Effects;
        public int Height;
        public int Offset;
        public int TextColor;
        public byte CharacterSet;
        public byte PitchAndFamily;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }
}
