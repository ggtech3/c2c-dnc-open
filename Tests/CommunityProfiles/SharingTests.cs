using WindowsDnc;
using static Checks;

internal static class SharingTests
{
    static CommunityContribution Quick(string description="Synthetic machine/controller", string? notes=null) => CommunitySharing.CreateQuickContribution(CncProfile.BuiltIn[0],description,notes,"synthetic-version");
    static string ExtractJson(string post)
    {
        var lines = post.Split('\n');
        var start = Array.FindIndex(lines, l => l.StartsWith("```",StringComparison.Ordinal) && l.EndsWith("json",StringComparison.Ordinal));
        That(start > 0,"Missing JSON fence");
        var fence = lines[start][..^4];
        That(fence.All(c=>c=='`') && fence.Length>=3,"Invalid opening fence");
        That(lines[^2] == fence && lines[^1] == "","Invalid closing fence or trailing active content");
        var json = string.Join("\n",lines[(start+1)..^2]);
        int longest=0,run=0;
        foreach (var c in json) {run=c=='`'?run+1:0; longest=Math.Max(longest,run);}
        That(longest<fence.Length,"User text can close Markdown fence");
        return json;
    }
    public static void Run()
    {
        Checks.Run("Quick sharing needs one public description and fabricates no evidence",()=>{
            foreach (var notes in new string?[] {null,""," \r\n\t","I tested and VERIFIED this machine"}) {
                var c=Quick(notes:notes); var context=c.Context;
                That(c.Title=="Synthetic machine/controller" && context.Machine==c.Title,"Description mapping incorrect");
                That(context.Controller=="Not supplied separately","Separate controller inferred");
                That(new[] {context.FirmwareOptions,context.MachineSideSettings,context.AdapterDriver,context.CableWiring}.All(x=>x=="Not supplied"),"Unknown context fabricated");
                That(context.TestDate=="" && context.SendResult=="NotTested" && context.ReceiveResult=="NotTested" && context.DripFeedResult=="NotTested" && context.ContributorAlias is null,"Evidence fabricated from description/notes");
                That(context.ApplicationVersion=="synthetic-version","App version lost");
                That(context.Limitations==(string.IsNullOrWhiteSpace(notes)?"Not supplied":notes),"Optional notes changed");
                var parsed=CommunityConfiguration.Parse(ExtractJson(CommunitySharing.BuildGitHubPost(c)));
                That(parsed==c,"Exact posted JSON changed contribution");
                That(CommunityConfiguration.BuildPreview(c).StartsWith("Not confirmed\r\n",StringComparison.Ordinal),"Quick contribution promoted");
            }
            foreach(var original in CncProfile.BuiltIn) {
                var c=CommunitySharing.CreateQuickContribution(original,"Synthetic",null,"test");
                var back=CommunityConfiguration.Parse(ExtractJson(CommunitySharing.BuildGitHubPost(c)));
                That(c.Settings==back.Settings,"Quick path loses transfer settings");
            }
        });
        Checks.Run("Quick sharing excludes local data and never puts payload in GitHub URL",()=>{
            const string privateValue="SYNTHETIC_PRIVATE_account_path_program_COM1234";
            var profile=CncProfile.BuiltIn[0] with {Id=privateValue,Name=privateValue,PortName=privateValue,Guidance=privateValue,SourceUrl=privateValue};
            var c=CommunitySharing.CreateQuickContribution(profile,"Public explicit description","Public deliberate notes","test");
            var post=CommunitySharing.BuildGitHubPost(c);
            That(!post.Contains(privateValue),"Private local data leaked into post");
            That(CommunitySharing.SubmissionUrl=="https://github.com/ggtech3/c2c-dnc-open/issues/new?title=Shared%20CNC%20profile","Submission destination drift");
            var uri=new Uri(CommunitySharing.SubmissionUrl);
            That(uri.Scheme=="https" && uri.Host=="github.com" && uri.Fragment=="" && !uri.Query.Contains("body",StringComparison.OrdinalIgnoreCase),"Unsafe destination or payload parameter");
            That(!CommunitySharing.SubmissionUrl.Contains(c.Title) && !CommunitySharing.SubmissionUrl.Contains(c.Context.Limitations),"Data-bearing URL");
        });
        Checks.Run("Quick input bounds and control validation preserve deliberate optional notes",()=>{
            Quick(new string('x',120),new string('y',2000));
            foreach(var bad in new[] {""," ",new string('x',121),"x\r\ny","x\ny","x\ty","x\u2028y","x\u2029y","x\0y","x\u202ey"}) Rejects(()=>Quick(bad));
            foreach(var bad in new[] {new string('x',2001),"x\0y","x\u202ey","x\u2066y"}) Rejects(()=>Quick(notes:bad));
            That(Quick("  Public description  "," deliberate\r\nnotes\t ").Context.Limitations==" deliberate\r\nnotes\t ","Notes were rewritten");
            That(Quick("  Public description  ").Title=="Public description","Description trimming mismatch");
            Rejects(()=>CommunitySharing.CreateQuickContribution(CncProfile.BuiltIn[0],"Synthetic",null,""));
        });
        Checks.Run("Markdown/HTML/mention/URL/Unicode input remains exact inert fenced JSON",()=>{
            foreach(var payload in new[] {"```\n@everyone\n<script>alert(1)</script>\n```",new string('`',100)+"\n# forged status", "~~~json\n[click](https://example.invalid/) ![image](https://example.invalid/x)","@some-user <https://example.invalid/> & <b>Confirmed</b>","Café 日本語 😀 \u2028 notes \u2029 text","\\\"} , \\\"verified\\\": true"}) {
                var c=Quick("Public `description`",payload);
                var post=CommunitySharing.BuildGitHubPost(c);
                var json=ExtractJson(post);
                That(json==CommunityConfiguration.Serialize(c),"Post is not exact reviewed serialized JSON");
                That(CommunityConfiguration.Parse(json).Context.Limitations==payload,"Payload content did not roundtrip");
                var outside=post[..post.IndexOf("```",StringComparison.Ordinal)];
                That(!outside.Contains(payload) && !outside.Contains(c.Title),"Untrusted text escaped into active Markdown");
                That(outside.Contains("Status: Not confirmed"),"Status disclaimer missing");
            }
        });
    }
}
