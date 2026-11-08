namespace WindowsDnc;

public enum SendStartMode
{
    FullProgram,
    CursorLine,
    PreviousToolChange
}

public sealed class SendPreviewDialog : Form
{
    private readonly CncProfile profile;
    private readonly string programName;
    private readonly string source;
    private readonly int cursorLineStart;
    private readonly int previousToolStart;
    private readonly ComboBox startModeBox = new();
    private readonly RichTextBox previewBox = new();
    private readonly Label statisticsLabel = new();

    public SendStartMode StartMode { get; private set; }
    public string SourceToSend { get; private set; }

    public SendPreviewDialog(
        CncProfile profile,
        string programName,
        string source,
        int cursorPosition,
        NcDocumentIndex documentIndex,
        bool darkMode)
    {
        this.profile = profile;
        this.programName = programName;
        this.source = source;
        cursorLineStart = FindLineStart(source, cursorPosition);
        NcNavigationItem? tool = documentIndex.Items
            .Where(item => item.Kind == NcNavigationKind.ToolChange && item.CharacterIndex <= cursorPosition)
            .LastOrDefault();
        previousToolStart = tool is null ? -1 : FindLineStart(source, tool.CharacterIndex);
        SourceToSend = source;

        Text = "Preview transfer — C2C DNC";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 560);
        Size = new Size(900, 680);
        Font = new Font("Segoe UI", 9F);
        BuildInterface();
        ThemeManager.Apply(this, darkMode);
        UpdatePreview();
    }

    private void BuildInterface()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        Controls.Add(layout);

        layout.Controls.Add(new Label
        {
            Text = $"Program: {programName}   •   Machine: {profile.Name}   •   {profile.PortName}   •   {profile.BaudRate} baud",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI Semibold", 10F),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        var startRow = new FlowLayoutPanel { Dock = DockStyle.Fill };
        startRow.Controls.Add(new Label
        {
            Text = "Send:",
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 0)
        });
        startModeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        startModeBox.Width = 260;
        startModeBox.Items.Add("Entire program");
        startModeBox.Items.Add("From current editor line");
        if (previousToolStart >= 0)
        {
            startModeBox.Items.Add("From preceding tool change");
        }
        startModeBox.SelectedIndex = 0;
        startModeBox.SelectedIndexChanged += (_, _) => UpdatePreview();
        startRow.Controls.Add(startModeBox);
        layout.Controls.Add(startRow, 0, 1);

        statisticsLabel.Dock = DockStyle.Fill;
        statisticsLabel.Tag = "muted";
        layout.Controls.Add(statisticsLabel, 0, 2);

        previewBox.Dock = DockStyle.Fill;
        previewBox.ReadOnly = true;
        previewBox.WordWrap = false;
        previewBox.Font = new Font("Cascadia Mono", 9.5F);
        layout.Controls.Add(previewBox, 0, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };
        var sendButton = new Button { Text = "Send", AutoSize = true };
        sendButton.Tag = "primary";
        sendButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };
        buttons.Controls.Add(sendButton);
        buttons.Controls.Add(cancelButton);
        layout.Controls.Add(buttons, 0, 4);
        AcceptButton = sendButton;
        CancelButton = cancelButton;
    }

    private void UpdatePreview()
    {
        if (startModeBox.SelectedIndex == 0)
        {
            StartMode = SendStartMode.FullProgram;
            SourceToSend = source;
        }
        else if (startModeBox.SelectedIndex == 1)
        {
            StartMode = SendStartMode.CursorLine;
            SourceToSend = source[cursorLineStart..];
        }
        else
        {
            StartMode = SendStartMode.PreviousToolChange;
            SourceToSend = source[previousToolStart..];
        }

        IReadOnlyList<string> lines = DncTextProcessor.PrepareForSend(SourceToSend, profile);
        string prepared = string.Join(profile.NewLine, lines);
        int transmittedBytes = lines.Sum(line =>
            System.Text.Encoding.ASCII.GetByteCount(line + profile.NewLine));
        previewBox.Text = prepared;
        statisticsLabel.Text =
            $"{lines.Count:N0} lines   •   {transmittedBytes:N0} transmitted bytes after profile filters";
    }

    private static int FindLineStart(string text, int position)
    {
        position = Math.Clamp(position, 0, text.Length);
        int newline = position == 0 ? -1 : text.LastIndexOf('\n', position - 1);
        return newline + 1;
    }
}
