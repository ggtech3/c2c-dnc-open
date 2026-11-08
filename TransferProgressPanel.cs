using System.ComponentModel;

namespace WindowsDnc;

public sealed class TransferProgressPanel : Panel
{
    private int progressPercent;
    private Color fillColor = Color.FromArgb(23, 104, 190);
    private bool marquee;
    private int marqueeOffset;
    private int progressAreaHeight = 64;

    public TransferProgressPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ProgressPercent
    {
        get => progressPercent;
        set { progressPercent = Math.Clamp(value, 0, 100); Invalidate(); }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor
    {
        get => fillColor;
        set { fillColor = value; Invalidate(); }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Marquee
    {
        get => marquee;
        set { marquee = value; Invalidate(); }
    }

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ProgressAreaHeight
    {
        get => progressAreaHeight;
        set { progressAreaHeight = Math.Max(1, value); Invalidate(); }
    }

    public void AdvanceMarquee()
    {
        marqueeOffset = (marqueeOffset + 5) % 130;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int fillHeight = Math.Min(progressAreaHeight, ClientSize.Height);
        if (marquee)
        {
            int width = Math.Max(90, ClientSize.Width / 5);
            int travel = ClientSize.Width + width;
            int x = marqueeOffset * travel / 130 - width;
            using var marqueeBrush = new SolidBrush(fillColor);
            e.Graphics.FillRectangle(marqueeBrush, x, 0, width, fillHeight);
        }
        else if (progressPercent > 0)
        {
            int width = (int)Math.Round(ClientSize.Width * progressPercent / 100d);
            using var progressBrush = new SolidBrush(fillColor);
            e.Graphics.FillRectangle(progressBrush, 0, 0, width, fillHeight);
        }
    }
}
