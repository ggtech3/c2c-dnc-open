using System.Reflection;

namespace WindowsDnc;

public sealed class AboutDialog : Form
{
    private const string PreviewText = """
        OPEN PREVIEW

        This private review build runs immediately, with no registration, activation, customer name, license key, purchase step, or transfer limit. It is based on C2C DNC 0.9.8 / v28.

        Your existing C2C DNC installation and its settings are separate. This preview stores its own preferences, profiles, and transfer log under %LOCALAPPDATA%\C2CDncOpenPreview.

        This is a product-behavior preview for the owner to review, not a public release or an open-source licensing grant. Distribution terms have not been decided.

        Verify every CNC program, machine profile, serial setting, and cable before use. This preview has not been validated on a live CNC machine.
        """;

    private const string SafetyText = """
        CNC AND RS-232 SAFETY DISCLAIMER

        C2C DNC transfers text between a Windows computer and CNC equipment. It does not determine whether a CNC program, communication setting, cable, machine setup, work offset, tool offset, spindle command, feed rate, rapid move, or toolpath is correct or safe.

        The operator is solely responsible for reviewing every program and confirming the selected CNC profile, serial settings, cable wiring, tools, offsets, workholding, material, clearances, and machine state before transfer or execution.

        Follow the machine manufacturer's procedures. Use available graphics, simulation, dry run, single block, feed hold, reduced rapid, and other verification features before production. Keep personnel clear of the machine and ensure emergency-stop controls are accessible.

        Machine and controller names are used only to describe compatibility. C2C DNC is not affiliated with or endorsed by EMCO, Siemens, Yasnac/Yaskawa, Haas, FANUC, Fagor, Mazak, Mitsubishi, Okuma, HEIDENHAIN, Fadal, Brother, or their respective owners. All trademarks belong to their owners.

        This disclaimer is a working draft and should be reviewed by a qualified lawyer before commercial release.
        """;

    public AboutDialog(bool darkMode)
    {
        Text = "About C2C DNC Open Preview";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(680, 520);
        Size = new Size(780, 620);
        ShowInTaskbar = false;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill };
        var logo = new PictureBox
        {
            Location = new Point(0, 2),
            Size = new Size(56, 56),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = BrandAssets.LoadLogo(darkMode)
        };
        header.Controls.Add(logo);
        header.Controls.Add(new Label
        {
            Text = "C2C DNC",
            Location = new Point(68, 2),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 20F)
        });
        header.Controls.Add(new Label
        {
            Text = $"Version {VersionText()}",
            Location = new Point(70, 42),
            AutoSize = true,
            Tag = "muted"
        });
        root.Controls.Add(header, 0, 0);

        string editionText = "OPEN PREVIEW\r\nNo registration or activation. Unlimited transfers.";
        var editionLabel = new Label
        {
            Text = editionText,
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 8, 12, 8),
            Font = new Font("Segoe UI Semibold", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Tag = "card"
        };
        root.Controls.Add(editionLabel, 0, 1);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreatePage("About", PreviewText));
        tabs.TabPages.Add(CreatePage("Safety Disclaimer", SafetyText));
        root.Controls.Add(tabs, 0, 2);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        var closeButton = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.OK,
            Size = new Size(96, 31)
        };
        buttonRow.Controls.Add(closeButton);
        root.Controls.Add(buttonRow, 0, 3);
        AcceptButton = closeButton;
        CancelButton = closeButton;

        ThemeManager.Apply(this, darkMode);
    }

    private static TabPage CreatePage(string title, string content)
    {
        var page = new TabPage(title);
        var text = new RichTextBox
        {
            Text = content,
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10F),
            BackColor = SystemColors.Window,
            DetectUrls = true,
            TabStop = false
        };
        page.Controls.Add(text);
        return page;
    }

    private static string VersionText()
    {
        Assembly assembly = typeof(AboutDialog).Assembly;
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "Unknown";
    }
}
