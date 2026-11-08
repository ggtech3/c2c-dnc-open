using WindowsDnc;

namespace CommunityUiSmoke;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var profiles = CncProfile.BuiltIn.ToList();
        using var launcher = new Form
        {
            Text = "Community UI smoke harness - synthetic / no profile storage",
            Width = 700, Height = 250, StartPosition = FormStartPosition.CenterScreen,
            Font = new Font("Segoe UI", 10F)
        };
        var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var status = new Label { Text = "Synthetic profile list only. No COM ports or real profile storage used.", AutoSize = true };
        var dark = new CheckBox { Text = "Dark theme", AutoSize = true };
        var share = new Button { Text = "Open Share setup", AutoSize = true };
        share.Click += (_, _) =>
        {
            using var dialog = new ShareProfileDialog(profiles[0] with { Name = "PRIVATE LOCAL TITLE", PortName = "COM4321", Guidance = "PRIVATE LOCAL NOTES", SourceUrl = "https://example.invalid/private" }, dark.Checked);
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
}
