namespace WindowsDnc;

/// <summary>Read-only source review followed by explicit creation of an independent local copy.</summary>
public sealed class CommunityProfilesDialog : Form
{
    private readonly ComboBox catalogBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly TextBox preview = new() { ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = true, Dock = DockStyle.Fill, AccessibleName = "Complete configuration and evidence preview" };
    private readonly TextBox nameBox = new() { Dock = DockStyle.Fill, MaxLength = 120, AccessibleName = "New local profile name" };
    private readonly TextBox portBox = new() { Dock = DockStyle.Fill, MaxLength = 7, AccessibleName = "Local COM port" };
    private readonly Button addButton = new() { Text = "Add profile", AutoSize = true, Enabled = false, Tag = "primary" };
    private readonly HashSet<string> existingNames;
    private CommunityContribution? contribution;
    private CommunityCatalogEntry? catalogEntry;
    public CncProfile? Result { get; private set; }

    public CommunityProfilesDialog(IEnumerable<CncProfile> existing, bool darkMode)
    {
        existingNames = existing.Select(p => p.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Text = "Community configurations - offline catalog / import";
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9F);
        Size = new Size(900, 760);
        MinimumSize = new Size(680, 500);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 6 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        Controls.Add(layout);
        layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Review every setting and the original test evidence before adding a new local copy.\r\nReports are not universal compatibility guarantees. Verify your machine, adapter, cable and settings.\r\nNo network access or machine operation occurs here." }, 0, 0);
        var source = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        source.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        source.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        source.Controls.Add(catalogBox, 0, 0);
        var import = new Button { Text = "Import contribution file…", Dock = DockStyle.Fill };
        import.Click += (_, _) => ImportFile();
        source.Controls.Add(import, 1, 0);
        layout.Controls.Add(source, 0, 1);
        layout.Controls.Add(preview, 0, 2);
        var local = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(0, 10, 0, 4) };
        local.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        local.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        local.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        local.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        local.Controls.Add(new Label { Text = "New local name", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        local.Controls.Add(nameBox, 1, 0);
        local.Controls.Add(new Label { Text = "Local COM port", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
        local.Controls.Add(portBox, 3, 0);
        layout.Controls.Add(local, 0, 3);
        layout.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Adding never replaces an existing profile. Choose your own COM1-COM9999.\r\nProvenance describes the original contribution only; edited local copies are not verified." }, 0, 4);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 0, 0) };
        addButton.Click += (_, _) => AddProfile();
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        var help = new Button { Text = "How to contribute", AutoSize = true };
        help.Click += (_, _) => MessageBox.Show(this, "Select a local profile and choose Share setup in the main window. Enter public controller, machine, wiring and test details, review all fields, then save the contribution file.\r\n\r\nContact the maintainer through an already-established private channel to arrange manual review. Do not send customer programs, machine serial numbers or other private data. There is no public intake or automatic upload. Maintainer review is distinct from independent machine confirmation.", "How to contribute", MessageBoxButtons.OK, MessageBoxIcon.Information);
        buttons.Controls.Add(addButton);
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(help);
        layout.Controls.Add(buttons, 0, 5);
        CancelButton = cancel;
        catalogBox.SelectedIndexChanged += (_, _) => SelectCatalogEntry();
        LoadCatalog();
        ThemeManager.Apply(this, darkMode);
    }

    private void LoadCatalog()
    {
        try
        {
            IReadOnlyList<CommunityCatalogEntry> entries = CommunityCatalog.LoadEmbedded();
            foreach (CommunityCatalogEntry entry in entries)
                catalogBox.Items.Add(new CatalogChoice(entry));
            if (entries.Count > 0) catalogBox.SelectedIndex = 0;
            else
            {
                catalogBox.Items.Add("Reviewed catalog: no entries yet");
                catalogBox.SelectedIndex = 0;
                preview.Text = "NO REVIEWED COMMUNITY CONFIGURATIONS YET\r\n\r\nThe catalog is intentionally empty until real contributions and evidence have been reviewed. The 18 built-in starting profiles are unchanged and are not newly certified.\r\n\r\nYou can import a contribution file for review. External files are always unreviewed user reports, regardless of claims in their text.\r\n\r\nChoose How to contribute for the private manual review process.";
            }
        }
        catch (Exception ex) when (ex is FormatException or IOException or InvalidOperationException)
        {
            preview.Text = "The bundled catalog could not be loaded. No catalog configuration can be added.\r\n\r\n" + ex.Message + "\r\n\r\nYou may still import a contribution file for unreviewed preview.";
        }
    }

    private void SelectCatalogEntry()
    {
        catalogEntry = (catalogBox.SelectedItem as CatalogChoice)?.Entry;
        contribution = catalogEntry?.Contribution;
        addButton.Enabled = contribution is not null;
        if (catalogEntry is not null)
        {
            preview.Text = CommunityCatalog.BuildPreview(catalogEntry);
            nameBox.Clear();
            portBox.Clear();
        }
    }

    private void ImportFile()
    {
        using var file = new OpenFileDialog { Title = "Open an unreviewed contribution", Filter = "Community configuration (*.json)|*.json|All files (*.*)|*.*", CheckFileExists = true, Multiselect = false };
        if (file.ShowDialog(this) != DialogResult.OK) return;
        // Clear prior selection before parsing, so a rejected file cannot leave Add pointing at an older source.
        catalogBox.SelectedIndex = -1;
        catalogEntry = null;
        contribution = null;
        addButton.Enabled = false;
        nameBox.Clear();
        portBox.Clear();
        try
        {
            CommunityContribution parsed = CommunityConfiguration.ReadFile(file.FileName);
            preview.Text = "SOURCE: EXTERNAL FILE - UNREVIEWED USER REPORT\r\nClaims in the file do not establish maintainer review or independent confirmation.\r\n\r\n" + CommunityConfiguration.BuildPreview(parsed);
            contribution = parsed;
            addButton.Enabled = true;
        }
        catch (Exception ex) when (ex is FormatException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            preview.Text = "Import rejected. No profile has been added.\r\n\r\n" + ex.Message;
            MessageBox.Show(this, ex.Message, "Invalid contribution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void AddProfile()
    {
        if (contribution is null) return;
        try
        {
            string name = nameBox.Text.Trim();
            if (existingNames.Contains(name)) throw new FormatException("That profile name already exists (including hidden profiles). Choose a distinct local name.");
            Result = catalogEntry is null
                ? CommunityConfiguration.CreateLocalProfile(contribution, name, portBox.Text.Trim())
                : CommunityCatalog.CreateLocalProfile(catalogEntry, name, portBox.Text.Trim());
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (FormatException ex)
        {
            MessageBox.Show(this, ex.Message, "Check local profile details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private sealed record CatalogChoice(CommunityCatalogEntry Entry)
    {
        public override string ToString() => Entry.Contribution.Title + (Entry.Status == CommunityReviewStatus.IndependentlyConfirmed
            ? " - Independently confirmed" : " - Maintainer-reviewed user report");
    }
}
