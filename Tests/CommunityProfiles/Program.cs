using System.IO.Ports;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WindowsDnc;
using static Checks;

internal static class Program
{
    static ContributionContext Context => new("Synthetic machine", "Synthetic controller", "Unknown", "Synthetic setup", "Synthetic adapter", "Synthetic wiring", "test-only", "2026-11-08", "Passed", "NotTested", "NotTested", "Synthetic only, no machine validation", "Tester A");
    static CommunityContribution Fixture => CommunityConfiguration.CreateContribution(CncProfile.BuiltIn[0], "Synthetic public setup", Context);
    static string Json => CommunityConfiguration.Serialize(Fixture);
    static string Change(string json, string path, JsonNode? value, bool remove = false)
    {
        var root = JsonNode.Parse(json)!.AsObject();
        var parent = Parent(root, path);
        if (remove) parent.Remove(path.Split('.')[^1]); else parent[path.Split('.')[^1]] = value;
        return root.ToJsonString();
    }
    static void Reject(string json) => Rejects(() => CommunityConfiguration.Parse(json));
    static void TransferEqual(CncProfile expected, CncProfile actual)
    {
        var excluded = new HashSet<string> { "Id", "IsBuiltIn", "Name", "PortName", "Guidance", "SourceUrl", "CompatibilityRevision" };
        var properties = typeof(CncProfile).GetProperties().Where(p => p.GetMethod is { IsStatic: false } && p.SetMethod is not null && !excluded.Contains(p.Name)).ToArray();
        That(properties.Length == 18, "Update roundtrip test for changed profile transfer fields");
        foreach (var p in properties) That(Equals(p.GetValue(expected), p.GetValue(actual)), $"Transfer setting lost: {p.Name}");
    }
    static int Main()
    {
        Console.WriteLine("COMMUNITY CONFIGURATION SYNTHETIC CHECKS " + DateTime.UtcNow.ToString("O"));
        Run("All 18 defaults preserve every portable transfer property", () => {
            That(CncProfile.BuiltIn.Count == 18, "Baseline count changed");
            foreach (var p in CncProfile.BuiltIn) {
                var c = CommunityConfiguration.CreateContribution(p, "Synthetic title", Context);
                var parsed = CommunityConfiguration.Parse(CommunityConfiguration.Serialize(c));
                var local = CommunityConfiguration.CreateLocalProfile(parsed, "Local synthetic", "COM9999");
                TransferEqual(p, local);
                That(!local.IsBuiltIn && Guid.TryParse(local.Id, out _) && local.Id != p.Id, "Not a new custom identity");
            }
        });
        Run("Every transfer setting survives non-default boundary values", () => {
            var p = CncProfile.BuiltIn[0] with { BaudRate = 921600, DataBits = 5, Parity = Parity.Space, StopBits = StopBits.OnePointFive, Handshake = Handshake.RequestToSendXOnXOff, DtrEnable = false, RtsEnable = false, NewLine = "\r", LineDelayMilliseconds = 60000, ReceivePercentFramed = true, ReceiveIdleTimeoutSeconds = null, StripNulls = false, RemoveWhitespace = false, OmitParenthesisLines = false, OmitLinesContainingParenthesis = false, RemoveReceiveControlCharacters = false, CheckParity = false, ParityReplaceCharacter = 127 };
            TransferEqual(p, CommunityConfiguration.CreateLocalProfile(CommunityConfiguration.Parse(CommunityConfiguration.Serialize(CommunityConfiguration.CreateContribution(p,"Synthetic",Context))), "Local", "COM1"));
            var lower = p with { BaudRate = 50, DataBits = 8, LineDelayMilliseconds = 0, ReceiveIdleTimeoutSeconds = 1, ParityReplaceCharacter = 0, NewLine = "\n" };
            CommunityConfiguration.Validate(CommunityConfiguration.CreateContribution(lower, "Synthetic", Context));
            CommunityConfiguration.Validate(Fixture with { Settings = Fixture.Settings with { ReceiveIdleTimeoutSeconds = 3600 } });
        });
        Run("Export allowlist excludes local identity, port, guidance, source and private text", () => {
            const string secret = "SYNTHETIC_PRIVATE_SENTINEL_account_program_path";
            var p = CncProfile.BuiltIn[0] with { Id = secret, Name = secret, PortName = secret, Guidance = secret, SourceUrl = secret, CompatibilityRevision = 999 };
            var json = CommunityConfiguration.Serialize(CommunityConfiguration.CreateContribution(p, "Public deliberate title", Context));
            That(!json.Contains(secret), "Local data leaked");
            var fields = Members(JsonNode.Parse(json)!.AsObject()).Select(x => x.Key).ToArray();
            foreach (var forbidden in new[] { "id", "isBuiltIn", "portName", "name", "guidance", "sourceUrl", "compatibilityRevision", "program", "path", "account" }) That(!fields.Contains(forbidden), "Forbidden field " + forbidden);
            var deliberate = Fixture with { Context = Context with { Limitations = secret } };
            That(CommunityConfiguration.Serialize(deliberate).Contains(secret), "Deliberate user-entered context unexpectedly removed");
        });
        Run("Missing, null, unknown and duplicate members fail closed at every object level", () => {
            var root = JsonNode.Parse(Json)!.AsObject();
            foreach (var (path, _, key) in Members(root)) {
                if (key != "contributorAlias") Reject(Change(Json, path, null, remove:true));
                if (key is not ("contributorAlias" or "receiveIdleTimeoutSeconds")) Reject(Change(Json, path, null));
            }
            foreach (var path in new[] { "unexpected", "settings.command", "context.verified" }) Reject(Change(Json, path, JsonValue.Create("untrusted")));
            foreach (var key in new[] { "schemaVersion", "baudRate", "controller" }) {
                var node = Members(root).Single(x => x.Key == key).Parent[key];
                var token = JsonSerializer.Serialize(key) + ":";
                var compact = root.ToJsonString();
                Reject(compact.Replace(token, token + node!.ToJsonString() + "," + token, StringComparison.Ordinal));
            }
            Reject(Change(Json, "SchemaVersion", JsonValue.Create(1)));
            CommunityConfiguration.Parse(Change(Json, "context.contributorAlias", null, remove:true));
        });
        Run("Types, enum spellings, numeric encodings and transfer ranges reject invalid values", () => {
            foreach (var key in new[] { "parity", "stopBits", "handshake" })
                foreach (JsonNode? value in new JsonNode?[] { JsonValue.Create(0), JsonValue.Create("0"), JsonValue.Create("none"), JsonValue.Create("Unknown"), new JsonArray() }) Reject(Change(Json,"settings." + key,value));
            Reject(Change(Json,"settings.stopBits",JsonValue.Create("None")));
            foreach (var (key, min, max) in new[] { ("baudRate",50,921600), ("dataBits",5,8), ("lineDelayMilliseconds",0,60000), ("receiveIdleTimeoutSeconds",1,3600), ("parityReplaceCharacter",0,127) }) {
                Reject(Change(Json,"settings."+key,JsonValue.Create(min-1)));
                Reject(Change(Json,"settings."+key,JsonValue.Create(max+1)));
                Reject(Change(Json,"settings."+key,JsonValue.Create("1")));
                Reject(Change(Json,"settings."+key,JsonValue.Create(1.5)));
            }
            foreach (var (path, _, key) in Members(JsonNode.Parse(Json)!.AsObject()).Where(x => x.Parent[x.Key] is JsonValue v && v.TryGetValue<bool>(out _))) Reject(Change(Json,path,JsonValue.Create("true")));
            Reject(Change(Json,"settings.newLine",JsonValue.Create("CRLF")));
            Reject(Change(Change(Json,"settings.receivePercentFramed",JsonValue.Create(false)),"settings.receiveIdleTimeoutSeconds",null));
        });
        Run("Malformed JSON, unsupported schema, depth, text and invalid Unicode rejected", () => {
            foreach (var bad in new[] { "", "null", "[]", "{}", "{", Json + "{}", Json.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 1, //comment"), new string('[',17)+"0"+new string(']',17) }) Reject(bad);
            foreach (var v in new[] { 0, 2, -1 }) Reject(Change(Json,"schemaVersion",JsonValue.Create(v)));
            foreach (var field in new[] { "title", "context.controller", "context.machine", "context.cableWiring", "context.limitations" }) {
                Reject(Change(Json,field,JsonValue.Create("  ")));
                Reject(Change(Json,field,JsonValue.Create(new string('x',2001))));
                Reject(Change(Json,field,JsonValue.Create("text\0")));
                Reject(Change(Json,field,JsonValue.Create("text\u202e")));
            }
            Reject(Change(Json,"context.testDate",JsonValue.Create("2026-02-30")));
            Reject(Change(Json,"context.sendResult",JsonValue.Create("Verified")));
            Reject(Json + "\ud800");
            CommunityConfiguration.Validate(Fixture with { Context = Context with { SendResult = "NotTested", TestDate = "" } });
        });
        Run("64 KiB UTF-8 file/string boundary, BOM and invalid UTF-8 in isolated temp files", () => {
            var dir = Path.Combine(Path.GetTempPath(), "C2C-community-synthetic-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try {
                var count = Encoding.UTF8.GetByteCount(Json);
                var exact = Json + new string(' ', CommunityConfiguration.MaximumFileBytes-count);
                CommunityConfiguration.Parse(exact);
                Reject(exact + " ");
                var path = Path.Combine(dir,"synthetic.json");
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(exact));
                CommunityConfiguration.ReadFile(path);
                File.AppendAllText(path," "); Rejects(() => CommunityConfiguration.ReadFile(path));
                File.WriteAllBytes(path,new byte[] {0xef,0xbb,0xbf}.Concat(Encoding.UTF8.GetBytes(Json)).ToArray());
                CommunityConfiguration.ReadFile(path);
                File.WriteAllBytes(path,new byte[] {0xc3,0x28}); Rejects(() => CommunityConfiguration.ReadFile(path));
                var unicode = Change(Json,"title",JsonValue.Create("Synthetic é"));
                // A multibyte character proves the limit counts encoded bytes, not UTF-16 chars.
                unicode = unicode.Replace("\\u00E9", "é", StringComparison.OrdinalIgnoreCase);
                Reject(unicode + new string(' ',CommunityConfiguration.MaximumFileBytes-unicode.Length));
            } finally { Directory.Delete(dir,true); }
        });
        Run("Repeated imports are unique custom copies and never mutate source lists", () => {
            var originals = CncProfile.BuiltIn.ToArray();
            var before = JsonSerializer.Serialize(originals);
            var ids = new HashSet<string>(originals.Select(x=>x.Id));
            for (var i=0;i<20;i++) {
                var p = CommunityConfiguration.CreateLocalProfile(Fixture," Same local name "," com9 ");
                That(ids.Add(p.Id) && Guid.TryParse(p.Id,out _) && !p.IsBuiltIn,"Identity reused");
                That(p.Name == "Same local name" && p.PortName == "COM9" && p.SourceUrl == "" && p.CompatibilityRevision == 0,"Local metadata incorrect");
                That(p.Guidance.Contains("unreviewed") && p.Guidance.Contains("do not inherit confirmation"),"Missing caution");
            }
            That(before == JsonSerializer.Serialize(originals),"Original list changed");
            foreach (var port in new[] { "", "COM0", "COM10000", "COM1;run", "\\\\.\\COM1", "not-a-port" }) Rejects(()=>CommunityConfiguration.CreateLocalProfile(Fixture,"Local",port));
            Rejects(()=>CommunityConfiguration.CreateLocalProfile(Fixture," ","COM1"));
            var preview = CommunityConfiguration.BuildPreview(Fixture);
            That(preview.Contains("UNREVIEWED USER REPORT") && preview.Contains(Json),"Preview omits full packet or untrusted status");
        });
        Run("Persistence is save-before-return; failure and invalid candidates leave originals unchanged", () => {
            var original = CncProfile.BuiltIn.ToList();
            var before = JsonSerializer.Serialize(original);
            var imported = CommunityConfiguration.CreateLocalProfile(Fixture, "New synthetic copy", "COM1");
            int calls = 0;
            var candidate = CommunityConfiguration.SaveNewProfile(original, imported, values => {
                calls++;
                That(JsonSerializer.Serialize(original) == before, "Original changed before save");
                That(values.Count() == original.Count + 1 && values.Last() == imported, "Incomplete persisted candidate");
                That(values is ICollection<CncProfile> { IsReadOnly: true }, "Save callback can mutate candidate membership");
            });
            That(calls == 1 && candidate.Count == 19 && JsonSerializer.Serialize(original) == before, "Save success contract failed");
            foreach (Exception failure in new Exception[] { new IOException("Synthetic disk failure"), new UnauthorizedAccessException("Synthetic access failure") }) {
                try { CommunityConfiguration.SaveNewProfile(original, imported, _ => throw failure); throw new Exception("Save exception swallowed"); }
                catch (Exception actual) when (ReferenceEquals(actual,failure)) { }
                That(JsonSerializer.Serialize(original) == before, "Failed save mutated originals");
            }
            foreach (var bad in new[] { imported with { Id=original[0].Id }, imported with { Id=" " }, imported with { IsBuiltIn=true }, imported with { Name=" " + original[0].Name.ToUpperInvariant() + " " } })
                Rejects(()=>CommunityConfiguration.SaveNewProfile(original,bad,_=>throw new Exception("Invalid candidate reached persistence")));
            var second = CommunityConfiguration.CreateLocalProfile(Fixture,"Second synthetic copy","COM2");
            var repeated = CommunityConfiguration.SaveNewProfile(candidate,second,_=>{});
            That(repeated.Count == 20 && repeated.Select(x=>x.Id).Distinct().Count() == 20,"Repeated import overwrote a profile");
        });
        Run("Single-line public and local fields reject injected line separators", () => {
            foreach (var separator in new[] { "\r\n", "\n", "\t", "\u2028", "\u2029" }) {
                foreach (var field in new[] { "title", "context.machine", "context.controller", "context.applicationVersion", "context.contributorAlias" })
                    Reject(Change(Json,field,JsonValue.Create("Synthetic" + separator + "CONFIRMED")));
                Rejects(()=>CommunityConfiguration.CreateLocalProfile(Fixture,"Local" + separator + "CONFIRMED","COM1"));
                Rejects(()=>CommunityConfiguration.CreateLocalProfile(Fixture,"Local","COM1" + separator));
                var imported = CommunityConfiguration.CreateLocalProfile(Fixture,"Local","COM1") with { Name="Local" + separator + "CONFIRMED" };
                Rejects(()=>CommunityConfiguration.SaveNewProfile(CncProfile.BuiltIn,imported,_=>throw new Exception("Injected label reached persistence")));
            }
        });        CatalogTests.Run();
        return Finish();
    }
}
