using WindowsDnc;

namespace CommunityUiSmoke;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Contains("--check-header")) { HeaderLayoutChecks.Run(args); return; }
        if (args.Contains("--check-controls")) { CheckControls(); return; }
        var profiles = ProfileFixtures.Baseline.ToList();
        using var launcher = new Form
        {
            Text = "Community UI smoke harness - synthetic / no profile storage",
            Width = 700, Height = 250, StartPosition = FormStartPosition.CenterScreen,
            Font = new Font("Segoe UI", 10F)
        };
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var status = new Label { Text = "Synthetic profile list only. No COM ports or real profile storage used.", AutoSize = true };
        var dark = new CheckBox { Text = "Dark theme", AutoSize = true };
        var share = new Button { Text = "Open Share (simulated actions)", AutoSize = true };
        share.Click += (_, _) =>
        {
            using var dialog = new ShareProfileDialog(profiles[0] with { Name = "PRIVATE LOCAL TITLE", PortName = "COM4321", Guidance = "PRIVATE LOCAL NOTES", SourceUrl = "https://example.invalid/private" }, dark.Checked, copyText: _ => status.Text = "SIMULATED clipboard copy only; real clipboard unchanged.", openGitHub: () => status.Text = "SIMULATED GitHub launch only; no browser opened.");
            dialog.ShowInTaskbar = true; // Make each test dialog directly selectable by native accessibility tools.
            var result = dialog.ShowDialog(launcher);
            status.Text = $"Share dialog closed: {result}. Synthetic profiles: {profiles.Count}.";
        };
        var import = new Button { Text = "Open Community configurations", AutoSize = true };
        import.Click += (_, _) =>
        {
            using var dialog = new CommunityProfilesDialog(profiles, dark.Checked);
            dialog.ShowInTaskbar = true; // Make each test dialog directly selectable by native accessibility tools.
            var result = dialog.ShowDialog(launcher);
            if (result == DialogResult.OK && dialog.Result is { } profile)
                profiles = CommunityConfiguration.SaveNewProfile(profiles, profile, _ => { });
            status.Text = $"Community dialog closed: {result}. Synthetic profiles: {profiles.Count}.";
        };
        layout.Controls.Add(status);
        layout.Controls.Add(dark);
        layout.Controls.Add(share);
        layout.Controls.Add(import);
        launcher.Controls.Add(layout);
        Application.Run(launcher);
    }
    // Hidden construction only: no message loop, Show/ShowDialog, clipboard or browser.
    private static void CheckControls()
    {
        static IEnumerable<Control> Descendants(Control root)
        {
            foreach (Control control in root.Controls)
            {
                yield return control;
                foreach (var child in Descendants(control)) yield return child;
            }
        }
        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
        try
        {
            string temporary = Path.Combine(Path.GetTempPath(), "dnc-ui-" + Guid.NewGuid().ToString("N"));
            // Isolated runtime paths; do not enumerate or open serial ports.
            using (var empty = new MainForm(temporary, enumeratePorts: false))
            {
                Require(!empty.Visible, "Main form unexpectedly shown");
                var controls = Descendants(empty).ToArray();
                Require(controls.OfType<Button>().Any(b => b.AccessibleName == "Choose an offline verified setup"), "No first-run catalog action");
                Require(controls.OfType<ComboBox>().Where(b => b.DropDownStyle == ComboBoxStyle.DropDownList && b.DropDownWidth == 440).Single().SelectedIndex == -1, "Fresh machine automatically selected");
                Require(controls.OfType<Button>().Where(b => b.Text is "Send program" or "Receive").All(b => !b.Enabled), "Transfer enabled without profile");
            }
            foreach (bool dark in new[] { false, true })
            {
                int copies=0,opens=0;
                using (var share = new ShareProfileDialog(ProfileFixtures.Baseline[0] with { Name="PRIVATE", Guidance="PRIVATE" },dark,_=>copies++,()=>opens++))
                {
                    var boxes=Descendants(share).OfType<TextBox>().Where(x=>!x.ReadOnly).ToArray();
                    Require(boxes.Length==2 && boxes.All(x=>x.Text==""),"Sharing must have only two initially blank inputs");
                    Require(boxes.Single(x=>!x.Multiline).MaxLength==120 && boxes.Single(x=>x.Multiline).MaxLength==2000,"Input bounds changed");
                    Require(share.CancelButton is Button { DialogResult: DialogResult.Cancel },"Cancel not wired");
                    Require(!share.Visible && copies==0 && opens==0,"Constructing dialog has sharing effects");
                    share.DialogResult=DialogResult.Cancel;
                    share.Close();
                }
                Require(copies==0 && opens==0,"Closing/canceling invoked sharing callbacks");
                using var browse=new CommunityProfilesDialog(ProfileFixtures.Baseline,dark);
                Require(!browse.Visible && browse.Result is null,"Construction imported a profile");
                var catalog=Descendants(browse).OfType<ComboBox>().Single();
                Require(catalog.Items.Count==3 && catalog.SelectedIndex==-1,"Three offline profiles must be available without initial selection");
                Require(Descendants(browse).OfType<Button>().Single(b=>b.Text=="Add profile").Enabled==false,"Unselected catalog enables Add");
                Require(Descendants(browse).OfType<TextBox>().Any(b=>b.Text.Contains("Not confirmed")),"Unselected catalog trust caution missing");
                catalog.SelectedIndex=0;
                Require(Descendants(browse).OfType<TextBox>().Where(b=>!b.ReadOnly).All(b=>b.Text==""),"Catalog selection assigned a local name/port");
                Require(browse.Result is null && Descendants(browse).OfType<TextBox>().Any(b=>b.Text.Contains("User verified - original setup only")),"Preview changed trust or imported silently");
            }
            Console.WriteLine("PASS hidden light/dark control construction, fresh fields, cancellation and unselected verified catalog; no desktop or sharing side effects.");
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine("FAIL hidden control checks: " + ex);
            Environment.ExitCode=1;
        }
    }
}
