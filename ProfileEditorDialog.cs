using System.IO.Ports;

namespace WindowsDnc;

public sealed class ProfileEditorDialog : Form
{
    private readonly TextBox nameBox = new();
    private readonly TextBox portBox = new();
    private readonly NumericUpDown baudBox = new();
    private readonly NumericUpDown dataBitsBox = new();
    private readonly ComboBox parityBox = new();
    private readonly ComboBox stopBitsBox = new();
    private readonly ComboBox handshakeBox = new();
    private readonly CheckBox dtrBox = new();
    private readonly CheckBox rtsBox = new();
    private readonly ComboBox lineEndingBox = new();
    private readonly NumericUpDown lineDelayBox = new();
    private readonly ComboBox receiveModeBox = new();
    private readonly NumericUpDown idleTimeoutBox = new();
    private readonly CheckBox stripNullsBox = new();
    private readonly CheckBox removeWhitespaceBox = new();
    private readonly CheckBox omitParenthesisBox = new();
    private readonly CheckBox omitContainingParenthesisBox = new();
    private readonly CheckBox removeControlCharactersBox = new();
    private readonly CheckBox checkParityBox = new();
    private readonly NumericUpDown parityReplaceBox = new();
    private readonly CncProfile original;

    public CncProfile Result { get; private set; }

    public ProfileEditorDialog(CncProfile profile, bool creating, bool darkMode)
    {
        original = profile;
        Result = profile;
        Text = creating ? "Add CNC profile" : "Edit CNC profile";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(540, 760);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(246, 248, 251);

        BuildInterface();
        LoadProfile(profile);
        ThemeManager.Apply(this, darkMode);
    }

    private void BuildInterface()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 21,
            AutoScroll = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(layout);

        int row = 0;
        AddField(layout, "Profile name", nameBox, row++);
        AddField(layout, "COM port", portBox, row++);

        baudBox.Minimum = 50;
        baudBox.Maximum = 921600;
        baudBox.Increment = 50;
        AddField(layout, "Baud rate", baudBox, row++);

        dataBitsBox.Minimum = 5;
        dataBitsBox.Maximum = 8;
        AddField(layout, "Data bits", dataBitsBox, row++);

        ConfigureList(parityBox, Enum.GetValues<Parity>().Cast<object>());
        AddField(layout, "Parity", parityBox, row++);

        ConfigureList(stopBitsBox, new object[] { StopBits.One, StopBits.OnePointFive, StopBits.Two });
        AddField(layout, "Stop bits", stopBitsBox, row++);

        ConfigureList(handshakeBox, Enum.GetValues<Handshake>().Cast<object>());
        AddField(layout, "Flow control", handshakeBox, row++);

        dtrBox.Text = "Enabled";
        AddField(layout, "DTR", dtrBox, row++);
        rtsBox.Text = "Enabled";
        AddField(layout, "RTS", rtsBox, row++);

        lineEndingBox.DropDownStyle = ComboBoxStyle.DropDownList;
        lineEndingBox.Items.AddRange(new object[] { "CR/LF", "LF", "CR" });
        AddField(layout, "Line endings", lineEndingBox, row++);

        lineDelayBox.Minimum = 0;
        lineDelayBox.Maximum = 60000;
        lineDelayBox.Increment = 10;
        lineDelayBox.ThousandsSeparator = true;
        AddField(layout, "Line delay (ms)", lineDelayBox, row++);

        receiveModeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        receiveModeBox.Items.AddRange(new object[] { "Starts and ends with %", "Finish after idle timeout" });
        AddField(layout, "Receive completion", receiveModeBox, row++);

        idleTimeoutBox.Minimum = 1;
        idleTimeoutBox.Maximum = 3600;
        AddField(layout, "Idle timeout / fallback (seconds)", idleTimeoutBox, row++);

        stripNullsBox.Text = "Strip NUL characters";
        AddWideField(layout, stripNullsBox, row++);
        removeWhitespaceBox.Text = "Remove spaces and tabs";
        AddWideField(layout, removeWhitespaceBox, row++);
        omitParenthesisBox.Text = "Omit send lines beginning with (";
        AddWideField(layout, omitParenthesisBox, row++);
        omitContainingParenthesisBox.Text = "Omit send lines containing ( anywhere";
        AddWideField(layout, omitContainingParenthesisBox, row++);
        removeControlCharactersBox.Text = "Remove received control characters below ASCII 32";
        AddWideField(layout, removeControlCharactersBox, row++);
        checkParityBox.Text = "Replace received characters with parity errors";
        AddWideField(layout, checkParityBox, row++);
        parityReplaceBox.Minimum = 0;
        parityReplaceBox.Maximum = 127;
        AddField(layout, "Parity replacement (ASCII)", parityReplaceBox, row++);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 0, 0)
        };
        var saveButton = new Button
        {
            Text = "Save profile",
            AutoSize = true,
            DialogResult = DialogResult.None
        };
        saveButton.Click += (_, _) => SaveAndClose();
        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        layout.Controls.Add(buttons, 0, row);
        layout.SetColumnSpan(buttons, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void LoadProfile(CncProfile profile)
    {
        nameBox.Text = profile.Name;
        portBox.Text = profile.PortName;
        baudBox.Value = Math.Clamp(profile.BaudRate, (int)baudBox.Minimum, (int)baudBox.Maximum);
        dataBitsBox.Value = profile.DataBits;
        parityBox.SelectedItem = profile.Parity;
        stopBitsBox.SelectedItem = profile.StopBits;
        handshakeBox.SelectedItem = profile.Handshake;
        dtrBox.Checked = profile.DtrEnable;
        rtsBox.Checked = profile.RtsEnable;
        lineEndingBox.SelectedIndex = profile.NewLine switch
        {
            "\n" => 1,
            "\r" => 2,
            _ => 0
        };
        lineDelayBox.Value = Math.Clamp(
            profile.LineDelayMilliseconds,
            (int)lineDelayBox.Minimum,
            (int)lineDelayBox.Maximum);
        receiveModeBox.SelectedIndex = profile.ReceivePercentFramed ? 0 : 1;
        idleTimeoutBox.Value = Math.Clamp(
            profile.ReceiveIdleTimeoutSeconds ?? 5,
            (int)idleTimeoutBox.Minimum,
            (int)idleTimeoutBox.Maximum);
        stripNullsBox.Checked = profile.StripNulls;
        removeWhitespaceBox.Checked = profile.RemoveWhitespace;
        omitParenthesisBox.Checked = profile.OmitParenthesisLines;
        omitContainingParenthesisBox.Checked = profile.OmitLinesContainingParenthesis;
        removeControlCharactersBox.Checked = profile.RemoveReceiveControlCharacters;
        checkParityBox.Checked = profile.CheckParity;
        parityReplaceBox.Value = Math.Clamp(profile.ParityReplaceCharacter, 0, 127);
    }

    private static bool HasRequiredIdentity(string name, string port) =>
        !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(port);

    private void SaveAndClose()
    {
        string name = nameBox.Text.Trim();
        string port = portBox.Text.Trim().ToUpperInvariant();
        if (!HasRequiredIdentity(name, port))
        {
            MessageBox.Show(this, "Profile name and COM port are required.", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = original with
        {
            Name = name,
            PortName = port,
            BaudRate = (int)baudBox.Value,
            DataBits = (int)dataBitsBox.Value,
            Parity = (Parity)parityBox.SelectedItem!,
            StopBits = (StopBits)stopBitsBox.SelectedItem!,
            Handshake = (Handshake)handshakeBox.SelectedItem!,
            DtrEnable = dtrBox.Checked,
            RtsEnable = rtsBox.Checked,
            NewLine = lineEndingBox.SelectedIndex switch { 1 => "\n", 2 => "\r", _ => "\r\n" },
            LineDelayMilliseconds = (int)lineDelayBox.Value,
            ReceivePercentFramed = receiveModeBox.SelectedIndex == 0,
            ReceiveIdleTimeoutSeconds = (int)idleTimeoutBox.Value,
            StripNulls = stripNullsBox.Checked,
            RemoveWhitespace = removeWhitespaceBox.Checked,
            OmitParenthesisLines = omitParenthesisBox.Checked,
            OmitLinesContainingParenthesis = omitContainingParenthesisBox.Checked,
            RemoveReceiveControlCharacters = removeControlCharactersBox.Checked,
            CheckParity = checkParityBox.Checked,
            ParityReplaceCharacter = (int)parityReplaceBox.Value,
            CompatibilityRevision = Math.Max(original.CompatibilityRevision, 1)
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private static void ConfigureList(ComboBox box, IEnumerable<object> values)
    {
        box.DropDownStyle = ComboBoxStyle.DropDownList;
        box.Items.AddRange(values.ToArray());
    }

    private static void AddField(TableLayoutPanel layout, string label, Control control, int row)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        var fieldLabel = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(3, 3, 3, 5);
        layout.Controls.Add(fieldLabel, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static void AddWideField(TableLayoutPanel layout, Control control, int row)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        control.Dock = DockStyle.Fill;
        layout.Controls.Add(control, 0, row);
        layout.SetColumnSpan(control, 2);
    }
}
