using WindowsDnc;
using System.Text.Json;
using System.Text.Json.Serialization;
var root = Path.GetFullPath(args[0]);
var entries = new List<CommunityCatalogEntry>();
foreach(var p in ProfileFixtures.Baseline) {
 bool verified = entries.Count < 3;
 var context = new ContributionContext(p.Name, p.Name, "Not recorded; confirm your control revision", "Match every portable setting below with the machine", "Not recorded; verify your adapter/driver", "Not recorded; verify cable pinout", "Historical C2C DNC setup", "", "NotTested", "NotTested", "NotTested", (verified ? "Owner confirmed this known setup as working on 2026-11-15. Operation-by-operation results and independent test evidence were not supplied. Not universal validation. " : "UNVERIFIED STARTER ONLY. No hardware validation claimed. ") + (string.IsNullOrWhiteSpace(p.Guidance) ? "Match all controller and cable settings." : p.Guidance) + (p.Id == "emco-et342-siemens-810t" ? " Includes compatibility revision 1 settings previously matched to the known-working CIMCO configuration; verify receive/parity behavior for your machine." : "") + (p.SourceUrl.Length > 0 ? " Reference: " + p.SourceUrl : ""));
 var c = CommunityConfiguration.CreateContribution(p,p.Name,context);
 if(verified) entries.Add(new(p.Id,1,c,CommunityReviewStatus.UserVerified,"Project owner","2026-11-15","Owner reports this setup as working. Not independently confirmed; no operation-specific coverage is asserted.",null));
 else File.WriteAllText(Path.Combine(root,"profiles","starters",p.Id+".json"),CommunityConfiguration.Serialize(c)+Environment.NewLine);
}
var options=new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase,WriteIndented=true};options.Converters.Add(new JsonStringEnumConverter());
File.WriteAllText(Path.Combine(root,"Assets","community-configurations.json"),JsonSerializer.Serialize(new CommunityCatalogDocument(1,entries),options)+Environment.NewLine);
Console.WriteLine("Exported 15 Not confirmed starter files and 3 owner-verified setup catalog entries, no local COM ports.");
