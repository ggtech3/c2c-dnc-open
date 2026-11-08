using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WindowsDnc;
using static Checks;

internal static class CatalogTests
{
    static CommunityContribution Contribution => CommunityConfiguration.CreateContribution(CncProfile.BuiltIn[0], "Synthetic catalog fixture", new ContributionContext("Synthetic machine", "Synthetic controller", "Unknown", "Synthetic parameters", "Synthetic adapter", "Synthetic wiring", "test-only", "2026-11-08", "Passed", "NotTested", "NotTested", "Synthetic only", "Tester A"));
    static CommunityCatalogEntry Report => new("synthetic-report", 1, Contribution, CommunityReviewStatus.UserReported, "Synthetic reviewer", "2026-11-08", "Synthetic private review", null);
    static CommunityConfirmation Evidence => new("Tester B", "2026-11-08", "Synthetic independent observation", "Passed", "NotTested", "NotTested");
    static string Encode(params CommunityCatalogEntry[] entries)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Serialize(new CommunityCatalogDocument(1, entries),options);
    }
    public static void Run()
    {
        Checks.Run("Real embedded catalog is empty; synthetic reported/confirmed statuses stay distinct", () => {
            That(CommunityCatalog.LoadEmbedded().Count == 0,"Shipped catalog must not claim synthetic verification");
            var report = CommunityCatalog.Parse(Encode(Report)).Single();
            That(CommunityCatalog.BuildPreview(report).Contains("not independent confirmation"),"Report status overstated");
            var confirmed = Report with { Status = CommunityReviewStatus.IndependentlyConfirmed, Confirmation = Evidence };
            var parsed = CommunityCatalog.Parse(Encode(confirmed)).Single();
            var preview = CommunityCatalog.BuildPreview(parsed);
            That(preview.Contains("INDEPENDENTLY CONFIRMED ORIGINAL SETUP") && preview.Contains(Evidence.Evidence) && preview.Contains("Independent send: Passed"),"Confirmation evidence missing");
            var copy = CommunityCatalog.CreateLocalProfile(parsed,"Local","COM1");
            That(!copy.IsBuiltIn && copy.Guidance.Contains("do not inherit confirmation") && copy.Guidance.Contains("revision 1"),"Local copy improperly inherits certification");
        });
        Checks.Run("Independent confirmation requires separate tester, evidence and concrete operation results", () => {
            var confirmed = Report with { Status = CommunityReviewStatus.IndependentlyConfirmed, Confirmation = Evidence };
            foreach (var bad in new CommunityConfirmation?[] { null, Evidence with { Tester = " tester a " }, Evidence with { Evidence = " " }, Evidence with { Tester = "" }, Evidence with { TestDate = "2026-02-30" }, Evidence with { SendResult = "NotTested" }, Evidence with { ReceiveResult = "Verified" } }) Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { Confirmation=bad })));
            Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { Contribution = Contribution with { Context = Contribution.Context with { ContributorAlias = null } } })));
            Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { ReviewDate = "2026-11-07" })));
            Rejects(()=>CommunityCatalog.Parse(Encode(Report with { Confirmation = Evidence })));
            // Neither absence of testing nor failures alone establish a working configuration.
            foreach (var send in new[] { "Failed", "NotTested" })
                foreach (var receive in new[] { "Failed", "NotTested" })
                    foreach (var drip in new[] { "Failed", "NotTested" })
                        Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { Confirmation = Evidence with { SendResult=send, ReceiveResult=receive, DripFeedResult=drip } })));
            // A pass must corroborate the same operation, not a different contributor claim.
            Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { Confirmation = Evidence with { SendResult="NotTested", ReceiveResult="Passed" } })));
            Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { Contribution = Contribution with { Context = Contribution.Context with { SendResult="Failed" } } })));
            var receiveConfirmed = confirmed with {
                Contribution = Contribution with { Context = Contribution.Context with { SendResult="NotTested", ReceiveResult="Passed" } },
                Confirmation = Evidence with { SendResult="NotTested", ReceiveResult="Passed" }
            };
            CommunityCatalog.Parse(Encode(receiveConfirmed));
            var dripConfirmed = confirmed with {
                Contribution = Contribution with { Context = Contribution.Context with { SendResult="NotTested", DripFeedResult="Passed" } },
                Confirmation = Evidence with { SendResult="NotTested", DripFeedResult="Passed" }
            };
            CommunityCatalog.Parse(Encode(dripConfirmed));
            // A confirmed send does not hide a separate failed receive operation.
            var mixed = confirmed with { Confirmation = Evidence with { ReceiveResult = "Failed" } };
            That(CommunityCatalog.BuildPreview(CommunityCatalog.Parse(Encode(mixed)).Single()).Contains("Independent receive: Failed"),"Failure evidence was hidden");
        });
        Checks.Run("Catalog reviewer and tester labels reject injected line separators", () => {
            var confirmed = Report with { Status = CommunityReviewStatus.IndependentlyConfirmed, Confirmation = Evidence };
            foreach (var separator in new[] { "\r\n", "\n", "\t", "\u2028", "\u2029" }) {
                Rejects(()=>CommunityCatalog.Parse(Encode(Report with { Reviewer="Reviewer" + separator + "INDEPENDENTLY CONFIRMED" })));
                Rejects(()=>CommunityCatalog.Parse(Encode(confirmed with { Confirmation=Evidence with { Tester="Tester B" + separator + "INDEPENDENTLY CONFIRMED" } })));
            }
        });        Checks.Run("Catalog duplicate IDs, malformed revisions, missing/null/unknown fields reject", () => {
            foreach (var id in new[] { "", "Uppercase", "bad--id", "../path", "a b" }) Rejects(()=>CommunityCatalog.Parse(Encode(Report with { Id=id })));
            foreach (var revision in new[] {0,-1}) Rejects(()=>CommunityCatalog.Parse(Encode(Report with {Revision=revision})));
            Rejects(()=>CommunityCatalog.Parse(Encode(Report,Report with {Revision=2})));
            foreach (var bad in new[] { "null", "{}", "{\"schemaVersion\":1,\"entries\":null}", "{\"schemaVersion\":1,\"entries\":[null]}", "{\"schemaVersion\":2,\"entries\":[]}", "{\"schemaVersion\":1,\"entries\":[],\"entries\":[]}", "{\"schemaVersion\":1,\"entries\":[],\"verified\":true}" }) Rejects(()=>CommunityCatalog.Parse(bad));
            var source = JsonNode.Parse(Encode(Report))!.AsObject();
            foreach (var key in source["entries"]![0]!.AsObject().Select(p=>p.Key).ToArray()) {
                var root = JsonNode.Parse(Encode(Report))!; root["entries"]![0]!.AsObject().Remove(key);
                Rejects(()=>CommunityCatalog.Parse(root.ToJsonString()));
            }
            foreach (var value in new JsonNode[] { JsonValue.Create(1.5)!,JsonValue.Create("1")!,JsonValue.Create(2147483648L)! }) {
                var root = JsonNode.Parse(Encode(Report))!; root["entries"]![0]!["revision"] = value;
                Rejects(()=>CommunityCatalog.Parse(root.ToJsonString()));
            }
            Rejects(()=>CommunityConfiguration.Parse(Encode(Report)));
            var file = JsonNode.Parse(CommunityConfiguration.Serialize(Contribution))!;
            file["status"]="IndependentlyConfirmed";
            Rejects(()=>CommunityConfiguration.Parse(file.ToJsonString()));
        });
    }
}
