using System.Text.Json;
using WindowsDnc;
using static Checks;
internal static class StarterLibraryTests
{
 public static void Run() {
  Checks.Run("Fresh installation has no machine profile and does not create storage",()=>{
   string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+"-profiles.json");
   That(CncProfile.BuiltIn.Count==0 && new ProfileRepository(path).Load().Count==0 && !File.Exists(path),"Fresh install must remain unconfigured");
   var blank=CncProfile.NewBlank(); That(!blank.IsBuiltIn && blank.Guidance.Contains("Not confirmed"),"Blank editor is not verified");
  });
  Checks.Run("Legacy defaults and user-imported profiles preserve all settings on load and save",()=>{
   string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+"-profiles.json");
   try {
    var old=ProfileFixtures.Baseline[0] with {BaudRate=4800,PortName="COM999",Guidance="my custom local note",CompatibilityRevision=0};
    var custom=ProfileFixtures.Baseline[1] with {Id="my-import",IsBuiltIn=false,Name="My import",PortName="COM4321"};
    File.WriteAllText(path,JsonSerializer.Serialize(new[]{old,custom}));var before=File.ReadAllText(path);
    var repo=new ProfileRepository(path);var loaded=repo.Load();
    That(loaded.Count==2 && loaded[0]==(old with {IsBuiltIn=false,Guidance="Legacy local profile - Not confirmed. "+old.Guidance}) && loaded[1]==custom,"Legacy data changed or dropped");
    That(File.ReadAllText(path)==before,"Read modified original file");
    repo.Save(loaded); That(repo.Load().SequenceEqual(loaded),"Restart changed local profiles");
    repo.Save(Array.Empty<CncProfile>());That(repo.Load().Count==0,"Empty store restored removed starters");
   } finally {if(File.Exists(path))File.Delete(path);}
  });
  Checks.Run("Three offline verified setups preserve exact historical values without local COM identifiers",()=>{
   var catalog=CommunityCatalog.LoadEmbedded();That(catalog.Count==3,"Catalog count");
   foreach(var e in catalog){var original=ProfileFixtures.Baseline.Single(p=>p.Id==e.Id);
    That(e.Contribution.Settings==CommunityConfiguration.CreateContribution(original,e.Contribution.Title,e.Contribution.Context).Settings,"Verified setup settings changed");
    That(e.Status==CommunityReviewStatus.UserVerified && e.Confirmation is null,"Owner report falsely independent");
    var copy=CommunityCatalog.CreateLocalProfile(e,"Local setup","COM123");That(copy.PortName=="COM123" && !copy.IsBuiltIn && copy.Guidance.Contains("do not inherit confirmation"),"Local copy trust/port");
   }
  });
  Checks.Run("15 repository-only starters preserve historical transfer settings, no trust or local port",()=>{
   string root=Directory.GetCurrentDirectory();while(!Directory.Exists(Path.Combine(root,"profiles","starters"))) root=Directory.GetParent(root)?.FullName ?? throw new Exception("Starter root missing");
   var files=Directory.GetFiles(Path.Combine(root,"profiles","starters"),"*.json");That(files.Length==15,"Starter count");
   foreach(var file in files){var c=CommunityConfiguration.ReadFile(file);var original=ProfileFixtures.Baseline.Single(p=>p.Id==Path.GetFileNameWithoutExtension(file));
    var expected=CommunityConfiguration.CreateContribution(original,c.Title,c.Context);
    That(c.Settings==expected.Settings,"Transfer defaults changed");That(c.Context.SendResult=="NotTested" && c.Context.ReceiveResult=="NotTested" && c.Context.DripFeedResult=="NotTested","Starter claims hardware proof");
    var local=CommunityConfiguration.CreateLocalProfile(c,"Local test","COM9999");That(!local.IsBuiltIn && local.Guidance.Contains("Not confirmed"),"Import trusted external text");
    That(!File.ReadAllText(file).Contains("portName",StringComparison.OrdinalIgnoreCase),"Local port leaked");
   }
  });
 }
}
