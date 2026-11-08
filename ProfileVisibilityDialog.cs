namespace WindowsDnc;

public sealed class ProfileVisibilityDialog : Form
{
    private readonly CheckedListBox profileList = new();
    private readonly IReadOnlyList<CncProfile> profiles;

    public HashSet<string> HiddenProfileIds { get; private set; }

    public ProfileVisibilityDialog(
        IReadOnlyList<CncProfile> profiles,
        IReadOnlySet<string> hiddenProfileIds,
        bool darkMode)
    {
        this.profiles = profiles;
        HiddenProfileIds = hiddenProfileIds.ToHashSet(StringComparer.Ordinal);
        Text = "Profiles shown in C2C DNC";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(560, 430);
        Font = new Font("Segoe UI", 9F);

        BuildInterface(hiddenProfileIds);
        ThemeManager.Apply(this, darkMode);
    }

    private void BuildInterface(IReadOnlySet<string> hiddenProfileIds)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(layout);

        layout.Controls.Add(new Label
        {
            Text = "Check the CNC profiles that should appear in the main profile dropdown.",
            Dock = DockStyle.Fill,
            AutoSize = false,
            Tag = "muted"
        }, 0, 0);

        profileList.Dock = DockStyle.Fill;
        profileList.CheckOnClick = true;
        profileList.IntegralHeight = false;
        for (int index = 0; index < profiles.Count; index++)
        {
            CncProfile profile = profiles[index];
            profileList.Items.Add(profile.Name, !hiddenProfileIds.Contains(profile.Id));
        }
        layout.Controls.Add(profileList, 0, 1);

        var selectionButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };
        var showAllButton = new Button { Text = "Show all", AutoSize = true };
        showAllButton.Click += (_, _) => SetAllChecked(true);
        var builtInsButton = new Button { Text = "Built-ins only", AutoSize = true };
        builtInsButton.Click += (_, _) =>
        {
            for (int index = 0; index < profiles.Count; index++)
            {
                profileList.SetItemChecked(index, profiles[index].IsBuiltIn);
            }
        };
        selectionButtons.Controls.Add(showAllButton);
        selectionButtons.Controls.Add(builtInsButton);
        layout.Controls.Add(selectionButtons, 0, 2);

        var actionButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        var saveButton = new Button { Text = "Apply", AutoSize = true };
        saveButton.Click += (_, _) => ApplyAndClose();
        var cancelButton = new Button
        {
            Text = "Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };
        actionButtons.Controls.Add(saveButton);
        actionButtons.Controls.Add(cancelButton);
        layout.Controls.Add(actionButtons, 0, 3);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void SetAllChecked(bool value)
    {
        for (int index = 0; index < profileList.Items.Count; index++)
        {
            profileList.SetItemChecked(index, value);
        }
    }

    private void ApplyAndClose()
    {
        if (profileList.CheckedIndices.Count == 0)
        {
            MessageBox.Show(
                this,
                "Keep at least one profile visible.",
                "C2C DNC",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        HiddenProfileIds = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < profiles.Count; index++)
        {
            if (!profileList.GetItemChecked(index))
            {
                HiddenProfileIds.Add(profiles[index].Id);
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
