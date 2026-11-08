namespace WindowsDnc;

/// <summary>Offline export of explicitly entered public context and portable settings only.</summary>
public sealed class ShareProfileDialog : Form
{
    private readonly CncProfile profile;
    private readonly bool darkMode;
    private readonly Dictionary<string, TextBox> fields = new();
    private readonly Dictionary<string, ComboBox> results = new();

    public ShareProfileDialog(CncProfile profile, bool darkMode)
    {
        this.profile = profile;
        this.darkMode = darkMode;
        Text = "Share setup - offline contribution";
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        Size = new Size(780, 760);
        MinimumSize = new Size(640, 480);
        Font = new Font("Segoe UI", 9F);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(root);
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Create a file for manual review. Nothing is uploaded.\r\nEnter public details afresh; do not include customer names, serial numbers, paths or programs.\r\nRequired details may say 'Unknown'. Use yyyy-MM-dd for test date, or leave blank if not tested.", AutoSize = false }, 0, 0);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var form = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 0, 18, 0) };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        scroll.Controls.Add(form);
        root.Controls.Add(scroll, 0, 1);
        AddField(form, "Public title", 120);
        AddField(form, "Machine", 200);
        AddField(form, "Controller", 200);
        AddField(form, "Firmware / options", 2000, true);
        AddField(form, "Machine-side settings", 2000, true);
        AddField(form, "Adapter / driver", 2000, true);
        AddField(form, "Cable / pinout", 2000, true);
        AddField(form, "Application version", 200);
        fields["Application version"].Text = Application.ProductVersion;
        AddField(form, "Test date (yyyy-MM-dd)", 10);
        AddResult(form, "Send result");
        AddResult(form, "Receive result");
        AddResult(form, "Drip-feed result");
        AddField(form, "Limitations / cautions", 2000, true);
        AddField(form, "Contributor alias (optional)", 120);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
        var review = new Button { Text = "Review export…", AutoSize = true, Tag = "primary" };
        review.Click += (_, _) => ReviewExport();
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(review);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 2);
        CancelButton = cancel;
        ThemeManager.Apply(this, darkMode);
    }

    private void AddField(TableLayoutPanel form, string label, int maxLength, bool multiline = false)
    {
        int row = form.RowCount++;
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, multiline ? 76 : 40));
        form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        var box = new TextBox { Dock = DockStyle.Fill, MaxLength = maxLength, Multiline = multiline, ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None, Margin = new Padding(3, 6, 3, 6), AccessibleName = label };
        fields.Add(label, box);
        form.Controls.Add(box, 1, row);
    }

    private void AddResult(TableLayoutPanel form, string label)
    {
        int row = form.RowCount++;
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        form.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        var box = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 6, 3, 6), AccessibleName = label };
        box.Items.AddRange(new object[] { "Not tested", "Passed", "Failed" });
        box.SelectedIndex = 0;
        results.Add(label, box);
        form.Controls.Add(box, 1, row);
    }

    private void ReviewExport()
    {
        string Value(string key) => fields[key].Text.Trim();
        string Outcome(string key) => results[key].SelectedIndex switch { 1 => "Passed", 2 => "Failed", _ => "NotTested" };
        try
        {
            var context = new ContributionContext(Value("Machine"), Value("Controller"), Value("Firmware / options"),
                Value("Machine-side settings"), Value("Adapter / driver"), Value("Cable / pinout"),
                Value("Application version"), Value("Test date (yyyy-MM-dd)"), Outcome("Send result"),
                Outcome("Receive result"), Outcome("Drip-feed result"), Value("Limitations / cautions"),
                string.IsNullOrWhiteSpace(Value("Contributor alias (optional)")) ? null : Value("Contributor alias (optional)"));
            CommunityContribution contribution = CommunityConfiguration.CreateContribution(profile, Value("Public title"), context);
            using var review = new Form { Text = "Review complete export before saving", StartPosition = FormStartPosition.CenterParent, Size = new Size(780, 720), MinimumSize = new Size(600, 420), ShowInTaskbar = false, MinimizeBox = false, Font = Font };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Privacy check: review every field below. Local profile identity, COM assignment, guidance and source links are excluded. Your entered text will be included. Saving creates a local file only." }, 0, 0);
            layout.Controls.Add(new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = true, Text = CommunityConfiguration.BuildPreview(contribution), AccessibleName = "Complete contribution preview" }, 0, 1);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
            var save = new Button { Text = "Save contribution…", AutoSize = true, DialogResult = DialogResult.OK, Tag = "primary" };
            var back = new Button { Text = "Back / cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(save);
            buttons.Controls.Add(back);
            layout.Controls.Add(buttons, 0, 2);
            review.Controls.Add(layout);
            review.CancelButton = back;
            ThemeManager.Apply(review, darkMode);
            if (review.ShowDialog(this) != DialogResult.OK) return;
            using var file = new SaveFileDialog { Title = "Save contribution locally", Filter = "Community configuration (*.json)|*.json", FileName = "community-configuration.json", DefaultExt = "json", AddExtension = true, OverwritePrompt = true };
            if (file.ShowDialog(this) != DialogResult.OK) return;
            string json = CommunityConfiguration.Serialize(contribution);
            string temporaryPath = file.FileName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                // Same-directory move preserves an existing destination if writing fails.
                File.WriteAllText(temporaryPath, json);
                File.Move(temporaryPath, file.FileName, true);
            }
            finally
            {
                try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
                catch (Exception cleanupError) when (cleanupError is IOException or UnauthorizedAccessException) { }
            }
            MessageBox.Show(this, "Contribution saved locally. Nothing was uploaded. Share it manually only after checking its contents.", "Contribution saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) when (ex is FormatException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message, "Could not export contribution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
