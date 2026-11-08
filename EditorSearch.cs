using System.Text.RegularExpressions;

namespace WindowsDnc;

public static class EditorSearch
{
    public static bool Find(
        RichTextBox editor,
        string term,
        bool forward,
        bool matchCase,
        bool wholeWord)
    {
        if (term.Length == 0 || editor.TextLength == 0)
        {
            return false;
        }

        string text = editor.Text;
        StringComparison comparison = matchCase
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        int start = forward
            ? Math.Min(text.Length, editor.SelectionStart + editor.SelectionLength)
            : Math.Max(0, editor.SelectionStart - 1);

        int index = FindIndex(text, term, start, forward, comparison, wholeWord);
        if (index < 0)
        {
            index = FindIndex(
                text,
                term,
                forward ? 0 : text.Length - 1,
                forward,
                comparison,
                wholeWord);
        }

        if (index < 0)
        {
            return false;
        }

        editor.Select(index, term.Length);
        editor.ScrollToCaret();
        editor.Focus();
        return true;
    }

    public static int ReplaceAll(
        RichTextBox editor,
        string term,
        string replacement,
        bool matchCase,
        bool wholeWord)
    {
        if (term.Length == 0)
        {
            return 0;
        }

        RegexOptions options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
        string pattern = Regex.Escape(term);
        if (wholeWord)
        {
            pattern = $@"(?<![A-Za-z0-9_]){pattern}(?![A-Za-z0-9_])";
        }

        var expression = new Regex(pattern, options);
        int count = expression.Matches(editor.Text).Count;
        if (count > 0)
        {
            int selectionStart = editor.SelectionStart;
            editor.Text = expression.Replace(editor.Text, _ => replacement);
            editor.SelectionStart = Math.Min(selectionStart, editor.TextLength);
        }
        return count;
    }

    public static bool SelectionMatches(
        RichTextBox editor,
        string term,
        bool matchCase) => string.Equals(
            editor.SelectedText,
            term,
            matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

    private static int FindIndex(
        string text,
        string term,
        int start,
        bool forward,
        StringComparison comparison,
        bool wholeWord)
    {
        int index = forward
            ? text.IndexOf(term, start, comparison)
            : text.LastIndexOf(term, Math.Min(start, Math.Max(0, text.Length - 1)), comparison);

        while (index >= 0 && wholeWord && !IsWholeWord(text, index, term.Length))
        {
            if (forward)
            {
                int next = index + Math.Max(1, term.Length);
                index = next >= text.Length ? -1 : text.IndexOf(term, next, comparison);
            }
            else
            {
                index = index == 0 ? -1 : text.LastIndexOf(term, index - 1, comparison);
            }
        }
        return index;
    }

    private static bool IsWholeWord(string text, int index, int length)
    {
        bool left = index == 0 || !IsWordCharacter(text[index - 1]);
        int end = index + length;
        bool right = end >= text.Length || !IsWordCharacter(text[end]);
        return left && right;
    }

    private static bool IsWordCharacter(char value) => char.IsLetterOrDigit(value) || value == '_';
}
