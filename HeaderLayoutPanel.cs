namespace WindowsDnc;

/// <summary>One non-wrapping action row; no runtime storage or serial dependencies.</summary>
public sealed class HeaderLayoutPanel : TableLayoutPanel
{
    private readonly FlowLayoutPanel actions;

    public HeaderLayoutPanel(Control branding, Control[] actionControls)
    {
        Dock = DockStyle.Fill;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ColumnCount = 2;
        RowCount = 1;
        ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        RowStyles.Add(new RowStyle(SizeType.AutoSize));

        branding.Dock = DockStyle.Fill;
        branding.Margin = Padding.Empty;
        Controls.Add(branding, 0, 0);

        actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(4, 0, 4, 0),
            AccessibleName = "Header actions"
        };
        actions.Controls.AddRange(actionControls);
        Controls.Add(actions, 1, 0);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        if (actions is not null)
        {
            // Always reserve scrollbar height, including when font/DPI changes make
            // the strip overflow. The action row must never wrap or clip vertically.
            int height = actions.Controls.Cast<Control>().Select(control =>
                Math.Max(control.Height, control.GetPreferredSize(Size.Empty).Height)
                + control.Margin.Vertical).DefaultIfEmpty(0).Max();
            actions.MinimumSize = new Size(0,
                height + actions.Padding.Vertical + SystemInformation.HorizontalScrollBarHeight);
            int width = actions.Controls.Cast<Control>().Sum(control =>
                Math.Max(control.Width, control.GetPreferredSize(Size.Empty).Width)
                + control.Margin.Horizontal);
            actions.AutoScrollMinSize = new Size(width + actions.Padding.Horizontal, 0);
        }
        base.OnLayout(levent);
    }
}
