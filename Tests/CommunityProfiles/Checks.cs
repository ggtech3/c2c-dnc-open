using System.Text.Json.Nodes;

internal static class Checks
{
    private static int passed;
    private static int failed;

    public static void Run(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine($"PASS {name}"); }
        catch (Exception ex) { failed++; Console.WriteLine($"FAIL {name}: {ex}"); }
    }

    public static void That(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Rejects(Action body)
    {
        try { body(); }
        catch (FormatException)
        { return; }
        throw new InvalidOperationException("Invalid input was accepted (or did not raise a validation exception).");
    }

    public static IEnumerable<(string Path, JsonObject Parent, string Key)> Members(JsonObject root, string path = "")
    {
        foreach (var item in root)
        {
            yield return (path + item.Key, root, item.Key);
            if (item.Value is JsonObject child)
                foreach (var nested in Members(child, path + item.Key + ".")) yield return nested;
        }
    }

    public static JsonObject Parent(JsonObject root, string dotted)
    {
        var names = dotted.Split('.');
        foreach (var name in names[..^1]) root = root[name]!.AsObject();
        return root;
    }

    public static int Finish()
    {
        Console.WriteLine($"{passed} passed; {failed} failed. Synthetic data only; no COM port or real profile store accessed.");
        return failed == 0 ? 0 : 1;
    }
}
