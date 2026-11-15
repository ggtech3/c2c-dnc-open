using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WindowsDnc;

/// <summary>Review a single sanitized profile before explicit clipboard/browser or local-file actions.</summary>
public sealed class ShareProfileDialog : Form
{
    private readonly CncProfile profile;
    private readonly bool darkMode;
    private readonly TextBox descriptionBox = new() { Dock = DockStyle.Fill, MaxLength = 120, AccessibleName = "Machine / controller description" };
    private readonly TextBox notesBox = new() { Dock = DockStyle.Fill, MaxLength = 2000, Multiline = true, ScrollBars = ScrollBars.Vertical, AccessibleName = "Notes (optional)" };
    private bool reviewing;
    private readonly Action<string> copyText;
    private readonly Action openGitHub;

    public ShareProfileDialog(CncProfile profile, bool darkMode, Action<string>? copyText = null, Action? openGitHub = null)
    {
        this.profile = profile;
        this.darkMode = darkMode;
        this.copyText = copyText ?? Clipboard.SetText;
        this.openGitHub = openGitHub ?? (() =>
        {
            using Process? process = Process.Start(new ProcessStartInfo(CommunitySharing.SubmissionUrl) { UseShellExecute = true });
        });
        Text = "Share profile";
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        Size = new Size(690, 470);
        MinimumSize = new Size(590, 420);
        Font = new Font("Segoe UI", 9F);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 6 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(root);
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Share this profile with the C2C DNC project on GitHub.\r\nGive the machine or controller a public description. You'll review everything before sharing." }, 0, 0);
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Machine / controller description (required)" }, 0, 1);
        root.Controls.Add(descriptionBox, 0, 2);
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Notes (optional) - for example, what worked or any limitations" }, 0, 3);
        root.Controls.Add(notesBox, 0, 4);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
        var review = new Button { Text = "Review", AutoSize = true, Tag = "primary" };
        review.Click += (_, _) => ReviewShare();
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(review);
        buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 5);
        CancelButton = cancel;
        ThemeManager.Apply(this, darkMode);
    }

    private void ReviewShare()
    {
        if (reviewing) return;
        reviewing = true;
        try
        {
            CommunityContribution contribution = CommunitySharing.CreateQuickContribution(profile,
                descriptionBox.Text.Trim(), notesBox.Text.Trim(), Application.ProductVersion);
            string post = CommunitySharing.BuildGitHubPost(contribution).ReplaceLineEndings("\r\n");
            using var review = new Form { Text = "Review your GitHub post", StartPosition = FormStartPosition.CenterParent, Size = new Size(820, 760), MinimumSize = new Size(740, 620), ShowInTaskbar = false, MinimizeBox = false, Font = Font };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            var privacy = new Label { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(3, 3, 3, 12), Text = "Recipient: ggtech3/c2c-dnc-open on public GitHub. Submitting requires a GitHub account; downloading does not.\r\n\r\nCheck the exact post below. Your local profile name, COM port, guidance, programs and logs are excluded. Remove private details from your description and notes.\r\n\r\nCopying replaces your clipboard; clipboard history or sync may retain this text. GitHub opens without your data. You must paste into the issue body and click Submit yourself." };
            void ConstrainPrivacyWidth() => privacy.MaximumSize = new Size(
                Math.Max(1, layout.ClientSize.Width - layout.Padding.Horizontal - privacy.Margin.Horizontal), 0);
            layout.SizeChanged += (_, _) => ConstrainPrivacyWidth();
            layout.Controls.Add(privacy, 0, 0);
            layout.Controls.Add(new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = true, Text = post, AccessibleName = "Exact GitHub post" }, 0, 1);
            var status = new Label { Dock = DockStyle.Fill, Text = "Nothing has been copied, opened or submitted." };
            layout.Controls.Add(status, 0, 2);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
            var share = new Button { Text = "Copy and open GitHub", AutoSize = true, Tag = "primary" };
            var copy = new Button { Text = "Copy only", AutoSize = true };
            var save = new Button { Text = "Save JSON copy", AutoSize = true };
            var back = new Button { Text = "Back", AutoSize = true, DialogResult = DialogResult.Cancel };
            bool acting = false;
            var dispatch = new ShareDispatch(copyText, openGitHub);
            void SetActing(bool value)
            {
                acting = value;
                share.Enabled = !value && !dispatch.HasOpened;
                copy.Enabled = save.Enabled = back.Enabled = !value;
            }
            share.Click += (_, _) =>
            {
                if (acting) return;
                SetActing(true);
                try
                {
                    ShareDispatchResult result = dispatch.TryCopyAndOpen(post);
                    status.Text = result switch
                    {
                        ShareDispatchResult.CopiedAndOpened => "Text copied and GitHub opening requested. Sign in if needed, paste into the issue body, then click Submit. Nothing has been submitted by C2C DNC.",
                        ShareDispatchResult.ClipboardFailed => "Could not copy the text. GitHub was not opened. Try again, or save a JSON copy.",
                        ShareDispatchResult.BrowserFailed => "Text copied, but GitHub could not be opened. Nothing was submitted. Try again, use Copy only, or save a JSON copy.",
                        ShareDispatchResult.AlreadyOpened => "GitHub opening was already requested. Paste the copied text into the issue body and click Submit yourself.",
                        _ => "A sharing action is already in progress."
                    };
                }
                finally { SetActing(false); }
            };
            copy.Click += (_, _) =>
            {
                if (acting) return;
                SetActing(true);
                try
                {
                    copyText(post);
                    status.Text = "Exact post copied. No browser was opened by this action and nothing was submitted. Clipboard history or sync may retain this text.";
                }
                catch (Exception ex) when (ex is ExternalException or ThreadStateException)
                {
                    status.Text = "Could not copy the text. Try again or save a JSON copy. No browser was opened by this action.";
                }
                finally { SetActing(false); }
            };
            save.Click += (_, _) =>
            {
                if (acting) return;
                SetActing(true);
                try { if (SaveCopy(review, contribution)) status.Text = "JSON copy saved locally. Nothing was submitted by C2C DNC."; }
                finally { SetActing(false); }
            };
            buttons.Controls.Add(share);
            buttons.Controls.Add(copy);
            buttons.Controls.Add(save);
            buttons.Controls.Add(back);
            layout.Controls.Add(buttons, 0, 3);
            review.Controls.Add(layout);
            review.CancelButton = back;
            review.FormClosing += (_, e) => { if (acting) e.Cancel = true; };
            ThemeManager.Apply(review, darkMode);
            review.ShowDialog(this);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message, "Check sharing details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { reviewing = false; }
    }

    private static bool SaveCopy(IWin32Window owner, CommunityContribution contribution)
    {
        using var file = new SaveFileDialog { Title = "Save contribution locally", Filter = "Community configuration (*.json)|*.json", FileName = "community-configuration.json", DefaultExt = "json", AddExtension = true, OverwritePrompt = true };
        if (file.ShowDialog(owner) != DialogResult.OK) return false;
        try
        {
            string json = CommunityConfiguration.Serialize(contribution);
            string temporaryPath = file.FileName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporaryPath, json);
                File.Move(temporaryPath, file.FileName, true);
            }
            finally
            {
                try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
                catch (Exception cleanupError) when (cleanupError is IOException or UnauthorizedAccessException) { }
            }
            return true;
        }
        catch (Exception ex) when (ex is FormatException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            MessageBox.Show(owner, ex.Message, "Could not save copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}
