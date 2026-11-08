using System.Text;

namespace WindowsDnc;

public static class DncTextProcessor
{
    public static IReadOnlyList<string> PrepareForSend(string source, CncProfile profile)
    {
        string normalized = NormalizeLineEndings(source);
        var output = new List<string>();

        foreach (string rawLine in normalized.Split('\n'))
        {
            string line = profile.StripNulls ? rawLine.Replace("\0", string.Empty) : rawLine;
            if (profile.OmitLinesContainingParenthesis && line.Contains('('))
            {
                continue;
            }
            if (profile.OmitParenthesisLines &&
                line.TrimStart().StartsWith("(", StringComparison.Ordinal))
            {
                continue;
            }

            if (profile.RemoveWhitespace)
            {
                line = RemoveHorizontalWhitespace(line);
            }

            output.Add(line);
        }

        // Split() creates one artificial final empty line when the source ends in a newline.
        if (normalized.EndsWith('\n') && output.Count > 0 && output[^1].Length == 0)
        {
            output.RemoveAt(output.Count - 1);
        }

        return output;
    }

    public static string CleanReceived(string source, CncProfile profile)
    {
        string cleaned = profile.StripNulls ? source.Replace("\0", string.Empty) : source;
        if (profile.RemoveReceiveControlCharacters)
        {
            var printable = new StringBuilder(cleaned.Length);
            foreach (char character in cleaned)
            {
                if (character >= ' ' || character is '\r' or '\n')
                {
                    printable.Append(character);
                }
            }
            cleaned = printable.ToString();
        }
        cleaned = NormalizeLineEndings(cleaned);

        if (profile.RemoveWhitespace)
        {
            string[] lines = cleaned.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = RemoveHorizontalWhitespace(lines[i]);
            }
            cleaned = string.Join(profile.NewLine, lines);
        }
        else
        {
            cleaned = cleaned.Replace("\n", profile.NewLine);
        }

        return cleaned;
    }

    public static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n").Replace('\r', '\n');

    private static string RemoveHorizontalWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            if (character != ' ' && character != '\t')
            {
                builder.Append(character);
            }
        }
        return builder.ToString();
    }
}
