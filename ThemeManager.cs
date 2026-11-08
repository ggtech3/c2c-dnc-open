namespace WindowsDnc;

public static class ThemeManager
{
    public static readonly Color Accent = Color.FromArgb(32, 111, 219);
    public static readonly Color Action = Color.FromArgb(15, 67, 116);

    public static Color Background(bool dark) => dark
        ? Color.FromArgb(14, 20, 29)
        : Color.FromArgb(242, 245, 249);

    public static Color Surface(bool dark) => dark
        ? Color.FromArgb(27, 37, 51)
        : Color.White;

    public static Color Input(bool dark) => dark
        ? Color.FromArgb(17, 25, 36)
        : Color.White;

    public static Color Foreground(bool dark) => dark
        ? Color.FromArgb(230, 237, 245)
        : Color.FromArgb(23, 39, 61);

    public static Color Muted(bool dark) => dark
        ? Color.FromArgb(159, 176, 195)
        : Color.FromArgb(91, 105, 122);

    public static Color Border(bool dark) => dark
        ? Color.FromArgb(66, 82, 103)
        : Color.FromArgb(190, 200, 213);

    public static void Apply(Control root, bool dark)
    {
        ApplyControl(root, dark);
        foreach (Control child in root.Controls)
        {
            Apply(child, dark);
        }
    }

    private static void ApplyControl(Control control, bool dark)
    {
        Color foreground = Foreground(dark);
        Color background = Background(dark);
        Color surface = Surface(dark);
        Color input = Input(dark);

        string tag = control.Tag as string ?? string.Empty;
        if (tag == "transfer-state")
        {
            control.ForeColor = ContrastColor(control.BackColor);
            return;
        }

        control.ForeColor = tag.Contains("muted", StringComparison.Ordinal) ? Muted(dark) : foreground;
        if (tag.Contains("transparent", StringComparison.Ordinal))
        {
            control.BackColor = Color.Transparent;
            return;
        }
        if (tag == "transfer-divider")
        {
            control.BackColor = Border(dark);
            return;
        }

        switch (control)
        {
            case Form:
                control.BackColor = background;
                break;
            case Panel or TableLayoutPanel or FlowLayoutPanel:
                control.BackColor = control.Tag as string == "card"
                    ? surface
                    : control.Parent?.BackColor ?? background;
                break;
            case RichTextBox richTextBox:
                richTextBox.BackColor = input;
                richTextBox.ForeColor = foreground;
                break;
            case TextBoxBase textBox:
                textBox.BackColor = input;
                textBox.ForeColor = foreground;
                break;
            case ComboBox comboBox:
                comboBox.BackColor = input;
                comboBox.ForeColor = foreground;
                break;
            case ListBox listBox:
                listBox.BackColor = input;
                listBox.ForeColor = foreground;
                break;
            case NumericUpDown numeric:
                numeric.BackColor = input;
                numeric.ForeColor = foreground;
                break;
            case Button button:
                if (tag.StartsWith("button-", StringComparison.Ordinal))
                {
                    Color groupColor = ButtonGroupColor(tag[7..]);
                    button.BackColor = groupColor;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = groupColor;
                }
                else if (button.Tag as string == "primary")
                {
                    button.BackColor = Action;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = Action;
                }
                else
                {
                    button.BackColor = surface;
                    button.ForeColor = foreground;
                    button.FlatAppearance.BorderColor = Border(dark);
                }
                break;
            default:
                control.BackColor = control.Parent?.BackColor ?? background;
                break;
        }
    }

    public static Color ButtonGroupColor(string group) => group switch
    {
        "file" => Color.FromArgb(22, 126, 91),
        "search" => Color.FromArgb(121, 74, 160),
        "navigation" => Color.FromArgb(42, 105, 176),
        "profile" => Color.FromArgb(184, 105, 24),
        "send" => Color.FromArgb(20, 145, 82),
        "receive" => Color.FromArgb(0, 125, 156),
        "danger" => Color.FromArgb(190, 55, 55),
        "utility" => Color.FromArgb(92, 82, 142),
        _ => Action
    };

    private static Color ContrastColor(Color background)
    {
        double luminance = 0.299 * background.R + 0.587 * background.G + 0.114 * background.B;
        return luminance > 155 ? Color.FromArgb(23, 39, 61) : Color.White;
    }
}
