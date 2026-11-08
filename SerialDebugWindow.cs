using System.Text;

namespace WindowsDnc;

public sealed class SerialDebugWindow : Form
{
    private const int MaximumCharacters = 250_000;
    private readonly RichTextBox debugText = new();
    private readonly CheckBox showLineEndingsBox = new() { Text = "Show CR/LF", Checked = true, AutoSize = true };
    private readonly List<(DateTime Time, string Direction, string Raw)> entries = [];
    private bool allowClose;

    public SerialDebugWindow(bool darkMode)
    {
        Text = "Raw serial debug — C2C DNC";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(850, 560);
        MinimumSize = new Size(600, 400);
        Font = new Font("Segoe UI", 9F);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(8, 7, 0, 0)
        };
        var clearButton = new Button { Text = "Clear", AutoSize = true };
        clearButton.Click += (_, _) => { entries.Clear(); debugText.Clear(); };
        var copyButton = new Button { Text = "Copy all", AutoSize = true };
        copyButton.Click += (_, _) =>
        {
            if (debugText.TextLength > 0)
            {
                Clipboard.SetText(debugText.Text);
            }
        };
        toolbar.Controls.Add(clearButton);
        toolbar.Controls.Add(copyButton);
        showLineEndingsBox.CheckedChanged += (_, _) => RenderAll();
        toolbar.Controls.Add(showLineEndingsBox);
        Controls.Add(toolbar);

        debugText.Dock = DockStyle.Fill;
        debugText.ReadOnly = true;
        debugText.WordWrap = false;
        debugText.Font = new Font("Cascadia Mono", 9F);
        Controls.Add(debugText);
        debugText.BringToFront();
        ThemeManager.Apply(this, darkMode);
    }

    public void Append(string direction, string rawText)
    {
        if (IsDisposed || rawText.Length == 0)
        {
            return;
        }

        entries.Add((DateTime.Now, direction, rawText));
        while (entries.Sum(item => item.Raw.Length) > MaximumCharacters && entries.Count > 1)
        {
            entries.RemoveAt(0);
        }
        AppendEntry(entries[^1]);
        debugText.SelectionStart = debugText.TextLength;
        debugText.ScrollToCaret();
    }

    public void ApplyTheme(bool darkMode) => ThemeManager.Apply(this, darkMode);

    public void ClosePermanently()
    {
        allowClose = true;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnFormClosing(e);
    }

    private void RenderAll()
    {
        debugText.Clear();
        foreach (var entry in entries) AppendEntry(entry);
        debugText.SelectionStart = debugText.TextLength;
        debugText.ScrollToCaret();
    }

    private void AppendEntry((DateTime Time, string Direction, string Raw) entry) =>
        debugText.AppendText($"[{entry.Time:HH:mm:ss.fff}] {entry.Direction}  {MakeControlsVisible(entry.Raw, showLineEndingsBox.Checked)}\r\n");

    private static string MakeControlsVisible(string value, bool showLineEndings)
    {
        var result = new StringBuilder(value.Length + 32);
        foreach (char character in value)
        {
            result.Append(character switch
            {
                '\r' => showLineEndings ? "<CR>" : string.Empty,
                '\n' => showLineEndings ? "<LF>\r\n" : "\r\n",
                '\0' => "<NUL>",
                '\u0011' => "<XON>",
                '\u0013' => "<XOFF>",
                _ when char.IsControl(character) => $"<0x{(int)character:X2}>",
                _ => character.ToString()
            });
        }
        return result.ToString();
    }
}
