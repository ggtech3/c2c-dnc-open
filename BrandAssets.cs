using System.Reflection;

namespace WindowsDnc;

public static class BrandAssets
{
    public static Image LoadLogo(bool darkMode)
    {
        string resourceName = darkMode ? "C2CDnc.LogoLight.png" : "C2CDnc.LogoDark.png";
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded logo resource {resourceName} was not found.");
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }
}
