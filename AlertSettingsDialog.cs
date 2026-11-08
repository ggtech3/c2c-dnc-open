namespace WindowsDnc;

public sealed class AlertSettingsDialog : Form
{
    private readonly CheckBox enabledBox = new();
    private readonly ComboBox completionBox = new();
    private readonly ComboBox failureBox = new();
    private readonly TrackBar volumeBar = new();

    public bool AlertsEnabled => enabledBox.Checked;
    public string CompletionSound => completionBox.SelectedItem as string ?? "Laundry Celebration";
    public string FailureSound => failureBox.SelectedItem as string ?? "Laundry Waltz";
    public int Volume => volumeBar.Value;

    public AlertSettingsDialog(
        bool enabled,
        string completionSound,
        string failureSound,
        int volume,
        bool darkMode)
    {
        Text = "Transfer sounds — C2C DNC";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 285);
        Font = new Font("Segoe UI", 9F);

        enabledBox.Text = "Play an alert when a transfer finishes";
        enabledBox.Checked = enabled;
        enabledBox.AutoSize = true;
        enabledBox.Location = new Point(22, 20);
        Controls.Add(enabledBox);

        AddSoundRow("Completed", completionBox, completionSound, 64);
        AddSoundRow("Failed / stopped", failureBox, failureSound, 112);

        Controls.Add(new Label { Text = "Volume", AutoSize = true, Location = new Point(22, 168) });
        volumeBar.Location = new Point(105, 154);
        volumeBar.Width = 255;
        volumeBar.Minimum = 0;
        volumeBar.Maximum = 100;
        volumeBar.TickFrequency = 10;
        volumeBar.Value = Math.Clamp(volume, 0, 100);
        Controls.Add(volumeBar);

        var previewButton = new Button { Text = "Preview", Location = new Point(370, 158), Size = new Size(82, 30) };
        previewButton.Click += (_, _) => AlertSoundService.Play(CompletionSound, Volume);
        Controls.Add(previewButton);

        var okButton = new Button { Text = "Save", DialogResult = DialogResult.OK, Location = new Point(274, 230), Size = new Size(84, 32), Tag = "primary" };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(368, 230), Size = new Size(84, 32) };
        Controls.Add(okButton);
        Controls.Add(cancelButton);
        AcceptButton = okButton;
        CancelButton = cancelButton;
        ThemeManager.Apply(this, darkMode);
    }

    private void AddSoundRow(string label, ComboBox box, string selected, int y)
    {
        Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(22, y + 7) });
        box.DropDownStyle = ComboBoxStyle.DropDownList;
        box.Items.AddRange(AlertSoundService.SoundNames.Cast<object>().ToArray());
        box.Location = new Point(130, y);
        box.Width = 230;
        box.SelectedItem = AlertSoundService.SoundNames.Contains(selected) ? selected : AlertSoundService.SoundNames[0];
        var preview = new Button { Text = "▶", Location = new Point(370, y), Size = new Size(50, 29) };
        preview.Click += (_, _) => AlertSoundService.Play(box.SelectedItem as string ?? "Shop Bell", Volume);
        Controls.Add(box);
        Controls.Add(preview);
    }
}
