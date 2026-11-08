namespace WindowsDnc;

public sealed class FlickerFreeTabControl : TabControl
{
    public FlickerFreeTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        UpdateStyles();
    }
}
