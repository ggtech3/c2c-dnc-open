namespace WindowsDnc;

public sealed class TransferLogRepository
{
    private readonly string filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "C2CDncOpenPreview",
        "transfer.log");

    public void Append(string direction, string profile, string result, string details)
    {
        string directory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(directory);
        string safeDetails = details.Replace('\r', ' ').Replace('\n', ' ');
        File.AppendAllText(
            filePath,
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{direction}\t{profile}\t{result}\t{safeDetails}{Environment.NewLine}");
    }

    public string ReadAll() => File.Exists(filePath)
        ? File.ReadAllText(filePath)
        : "No transfers have been logged yet.";
}

public sealed class TransferLogDialog : Form
{
    public TransferLogDialog(string contents, bool darkMode)
    {
        Text = "Transfer log — C2C DNC";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(900, 560);
        MinimumSize = new Size(650, 400);
        Font = new Font("Segoe UI", 9F);

        var text = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            WordWrap = false,
            Font = new Font("Cascadia Mono", 9F),
            Text = contents
        };
        Controls.Add(text);
        ThemeManager.Apply(this, darkMode);
    }
}
