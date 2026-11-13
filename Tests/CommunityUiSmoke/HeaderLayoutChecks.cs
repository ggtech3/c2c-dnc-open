using WindowsDnc;

namespace CommunityUiSmoke;

internal static class HeaderLayoutChecks
{
    public static void Run(string[] args)
    {
        string? output = args.SkipWhile(a => a != "--output").Skip(1).FirstOrDefault();
        if (output is not null) Directory.CreateDirectory(output);
        try
        {
            int cases = 0;
            foreach (float scale in new[] { 1F, 1.25F, 1.5F, 2F })
            foreach (bool dark in new[] { false, true })
            {
                // An unattached Panel uses hidden parked native handles while its
                // controls remain logically visible for real scrolling/layout.
                using var form = new Panel { Font = new Font("Segoe UI", 9F * scale) };
                var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Font = form.Font, Padding = new Padding((int)(22 * scale)) };
                root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                var branding = new Panel { Size = new Size((int)(330 * scale), (int)(76 * scale)), MinimumSize = new Size((int)(330 * scale), (int)(76 * scale)) };
                branding.Controls.Add(new Label { Text = "C2C DNC", Font = new Font("Segoe UI Semibold", 20F * scale), AutoSize = true, Location = new Point((int)(58 * scale), (int)(2 * scale)) });
                branding.Controls.Add(new Label { Text = "RS-232 program sender / receiver", Font = new Font("Segoe UI", 9.5F * scale), AutoSize = true, Location = new Point((int)(60 * scale), (int)(38 * scale)) });
                Button MakeButton(string text, int height, string group) => new()
                {
                    Text = text, AutoSize = true, Height = (int)(height * scale),
                    Font = new Font("Segoe UI Semibold", 9F * scale), FlatStyle = FlatStyle.Flat,
                    Margin = new Padding((int)(3 * scale)), Tag = "button-" + group
                };
                var about = MakeButton("About…", 30, "utility");
                var shown = MakeButton("Shown profiles…", 30, "profile");
                var theme = new CheckBox { Text = "Dark mode", AutoSize = true, Checked = dark, Margin = new Padding((int)(10 * scale), (int)(7 * scale), 0, 0) };
                var share = MakeButton("Share", 30, "profile");
                var community = MakeButton("Community configurations…", 30, "profile");
                var header = new HeaderLayoutPanel(branding, [about, shown, theme, share, community]);
                root.Controls.Add(header, 0, 0);
                var body = new Panel { Dock = DockStyle.Fill, Tag = "card" };
                root.Controls.Add(body, 0, 1);
                form.Controls.Add(root);
                ThemeManager.Apply(form, dark);
                Control[] actions = [about, shown, theme, share, community];
                int? minimumHeight = null;
                foreach (int logicalWidth in new[] { 1034, 1104, 1500, 700, 1034, 700, 1104, 1034 })
                {
                    form.ClientSize = new Size((int)(logicalWidth * scale), (int)(300 * scale));
                    Layout(form);
                    Rectangle BoundsIn(Control child, Control ancestor)
                    {
                        var result = child.Bounds;
                        for (Control? parent = child.Parent; parent is not null && parent != ancestor; parent = parent.Parent)
                            result.Offset(parent.Location);
                        return result;
                    }
                    foreach (Control action in actions)
                    {
                        for (Control? parent = action.Parent; parent is not null && parent != form; parent = parent.Parent)
                        {
                            Rectangle bounds = BoundsIn(action, parent);
                            Require(bounds.Top >= 0 && bounds.Bottom <= parent.ClientSize.Height, $"Vertically clipped {action.Text}, scale {scale}, width {logicalWidth}");
                            if (logicalWidth != 700)
                                Require(parent.ClientRectangle.Contains(bounds), $"Clipped {action.Text} in {parent.GetType().Name}, scale {scale}, width {logicalWidth}");
                        }
                        Require(action.Height >= action.GetPreferredSize(Size.Empty).Height, $"Insufficient button height: {action.Text}");
                    }
                    var strip = (FlowLayoutPanel)about.Parent!;
                    Require(!strip.WrapContents && actions.All(a => a.Parent == strip), "Actions must share one non-wrapping strip");
                    Require(!strip.Bounds.IntersectsWith(branding.Bounds), "Action viewport overlaps branding");
                    Require(header.Bottom <= body.Top, "Header overlaps profile content");
                    Require(new[] { about, shown, share, community }.All(b => b.Top == about.Top && b.Height == about.Height), "Buttons must align in one equal-height row");
                    if (logicalWidth == 700)
                    {
                        Require(strip.HorizontalScroll.Visible && !strip.VerticalScroll.Visible, "Narrow header needs only a horizontal scrollbar");
                        strip.AutoScrollPosition = new Point(strip.HorizontalScroll.Maximum, 0);
                        Require(community.Left >= 0 && community.Right <= strip.ClientSize.Width, $"Cannot reach last action by scrolling: bounds {community.Bounds}, viewport {strip.ClientSize}, position {strip.AutoScrollPosition}, handle {strip.IsHandleCreated}");
                        strip.AutoScrollPosition = Point.Empty;
                        Require(about.Left >= 0 && about.Right <= strip.ClientSize.Width, "Cannot return to first action by scrolling");
                    }
                    else Require(!strip.HorizontalScroll.Visible && !strip.VerticalScroll.Visible, "Normal width unexpectedly requires scrolling");
                    if (logicalWidth == 1034)
                    {
                        minimumHeight ??= header.Height;
                        Require(header.Height == minimumHeight, "Repeated resize accumulates header height");
                    }
                    cases++;
                }
                if (output is not null)
                {
                    // Render the unattached control tree without showing a desktop window.
                    form.Controls.Remove(root);
                    root.Dock = DockStyle.None;
                    root.Location = Point.Empty;
                    root.Size = form.ClientSize;
                    root.Visible = true;
                    Layout(root);
                    using var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height);
                    root.DrawToBitmap(bitmap, root.ClientRectangle);
                    bitmap.Save(Path.Combine(output, $"header-{(dark ? "dark" : "light")}-{scale * 100:0}.png"));
                    root.Dispose();
                }
            }
            Console.WriteLine($"PASS {cases} disconnected one-row header cases: four font/layout scales, light/dark, minimum/default/wide/constrained widths, horizontal scrolling and repeated resize. No user settings, serial service or desktop interaction.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FAIL header layout: " + ex);
            Environment.ExitCode = 1;
        }
    }

    private static void Layout(Control control)
    {
        // Force hidden native handles so scrollbar movement is exercised even
        // though no Form.Show or desktop message loop is used.
        _ = control.Handle;
        control.PerformLayout();
        foreach (Control child in control.Controls) Layout(child);
        control.PerformLayout();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
