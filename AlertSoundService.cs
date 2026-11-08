using System.Runtime.InteropServices;
using System.Text;

namespace WindowsDnc;

public static class AlertSoundService
{
    private const uint SndAsync = 0x0001;
    private const uint SndNodefault = 0x0002;
    private const uint SndFilename = 0x00020000;
    private const int SampleRate = 44100;

    public static IReadOnlyList<string> SoundNames { get; } =
    [
        "Shop Bell",
        "Victory",
        "Robot",
        "Bright Chime",
        "Arcade",
        "Three Cheers",
        "Laundry Waltz",
        "Laundry Celebration"
    ];

    public static void Play(string soundName, int volume)
    {
        (double Frequency, int Milliseconds)[] notes = soundName switch
        {
            "Victory" => [(523, 130), (659, 130), (784, 260)],
            "Robot" => [(330, 90), (495, 90), (660, 90), (495, 170)],
            "Bright Chime" => [(880, 120), (1175, 120), (1568, 260)],
            "Arcade" => [(392, 80), (523, 80), (659, 80), (784, 180)],
            "Three Cheers" => [(659, 110), (0, 45), (659, 110), (0, 45), (988, 240)],
            // Original appliance-style melodies. These are generated locally and
            // do not contain recordings or melodies copied from a manufacturer.
            "Laundry Waltz" =>
            [
                (659, 135), (784, 135), (988, 270), (0, 55),
                (784, 135), (659, 135), (587, 270), (0, 55),
                (659, 135), (740, 135), (880, 270), (0, 55),
                (587, 135), (659, 135), (784, 390)
            ],
            "Laundry Celebration" =>
            [
                (523, 105), (659, 105), (784, 105), (1047, 220), (0, 45),
                (988, 105), (784, 105), (880, 210), (0, 45),
                (659, 105), (784, 105), (988, 105), (1175, 220), (0, 45),
                (1047, 125), (1175, 125), (1319, 360)
            ],
            _ => [(784, 140), (1047, 300)]
        };

        volume = Math.Clamp(volume, 0, 100);
        string directory = GetWritableSoundDirectory();
        string safeName = new(soundName.Where(char.IsLetterOrDigit).ToArray());
        string path = Path.Combine(directory, $"{safeName}-{volume}.wav");
        if (!File.Exists(path))
        {
            File.WriteAllBytes(path, CreateWave(notes, volume));
        }
        PlaySound(path, IntPtr.Zero, SndFilename | SndAsync | SndNodefault);
    }

    private static string GetWritableSoundDirectory()
    {
        string preferred = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "C2CDnc",
            "sounds");
        try
        {
            Directory.CreateDirectory(preferred);
            return preferred;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            string fallback = Path.Combine(Path.GetTempPath(), "C2CDnc", "sounds");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }

    private static byte[] CreateWave(
        IReadOnlyList<(double Frequency, int Milliseconds)> notes,
        int volume)
    {
        var samples = new List<short>();
        double amplitude = short.MaxValue * 0.28 * volume / 100d;
        foreach ((double frequency, int milliseconds) in notes)
        {
            int count = SampleRate * milliseconds / 1000;
            int fade = Math.Min(SampleRate / 100, Math.Max(1, count / 5));
            for (int index = 0; index < count; index++)
            {
                double envelope = Math.Min(1d, Math.Min(index / (double)fade, (count - index - 1) / (double)fade));
                double wave = frequency <= 0
                    ? 0
                    : Math.Sin(2 * Math.PI * frequency * index / SampleRate);
                samples.Add((short)(amplitude * Math.Max(0, envelope) * wave));
            }
        }

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, true);
        int dataLength = samples.Count * sizeof(short);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataLength);
        writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * sizeof(short));
        writer.Write((short)sizeof(short));
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);
        foreach (short sample in samples)
        {
            writer.Write(sample);
        }
        writer.Flush();
        return stream.ToArray();
    }

    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    private static extern bool PlaySound(string sound, IntPtr module, uint flags);
}
