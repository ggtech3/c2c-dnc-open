using WindowsDnc;
using static Checks;

internal static class DispatchTests
{
    public static void Run()
    {
        Checks.Run("Explicit dispatch copies exact review before launch and suppresses repeat/reentry",()=>{
            var events=new List<string>();
            ShareDispatch? dispatch=null;
            dispatch=new ShareDispatch(text=>{
                events.Add("copy:"+text);
                That(dispatch!.TryCopyAndOpen("nested")==ShareDispatchResult.Busy,"Reentrant copy accepted");
            },()=>{
                events.Add("open");
                That(dispatch!.TryCopyAndOpen("nested")==ShareDispatchResult.Busy,"Reentrant launch accepted");
            });
            That(events.Count==0 && !dispatch.HasOpened,"Construction causes sharing effects");
            That(dispatch.TryCopyAndOpen("EXACT REVIEWED POST")==ShareDispatchResult.CopiedAndOpened,"Dispatch failed");
            That(events.SequenceEqual(new[] {"copy:EXACT REVIEWED POST","open"}),"Wrong order or copied text");
            That(dispatch.TryCopyAndOpen("different")==ShareDispatchResult.AlreadyOpened && events.Count==2,"Repeated click duplicated effects");
            That(dispatch.HasOpened && dispatch.LastError is null,"Success state incorrect");
        });
        Checks.Run("Clipboard failure prevents launch and explicit retry can succeed",()=>{
            foreach(Exception failure in new Exception[] {new System.Runtime.InteropServices.ExternalException("clipboard busy"),new ThreadStateException("not STA")}) {
                int copy=0,open=0;
                var dispatch=new ShareDispatch(_=>{if(++copy==1) throw failure;},()=>open++);
                That(dispatch.TryCopyAndOpen("review")==ShareDispatchResult.ClipboardFailed && open==0 && !dispatch.HasOpened,"Opened despite failed clipboard");
                That(dispatch.LastError==failure.Message,"Clipboard error lost");
                That(dispatch.TryCopyAndOpen("review")==ShareDispatchResult.CopiedAndOpened && copy==2 && open==1 && dispatch.LastError is null,"Clipboard retry failed");
            }
        });
        Checks.Run("Browser failure preserves copied outcome and allows explicit retry",()=>{
            foreach(Exception failure in new Exception[] {new System.ComponentModel.Win32Exception("browser unavailable"),new InvalidOperationException("launch failed"),new NotSupportedException("unsupported")}) {
                int copy=0,open=0;
                var dispatch=new ShareDispatch(_=>copy++,()=>{if(++open==1) throw failure;});
                That(dispatch.TryCopyAndOpen("review")==ShareDispatchResult.BrowserFailed && copy==1 && open==1 && !dispatch.HasOpened,"Browser failure state incorrect");
                That(dispatch.LastError==failure.Message,"Browser error lost");
                That(dispatch.TryCopyAndOpen("review")==ShareDispatchResult.CopiedAndOpened && copy==2 && open==2 && dispatch.LastError is null,"Explicit browser retry failed");
            }
        });
        Checks.Run("Concurrent clicks do not queue launches; unexpected callback errors release guard",()=>{
            using var entered=new ManualResetEventSlim(); using var release=new ManualResetEventSlim();
            int copies=0,opens=0;
            var dispatch=new ShareDispatch(_=>{Interlocked.Increment(ref copies);entered.Set();if(!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("Synthetic guard timeout");},()=>Interlocked.Increment(ref opens));
            var first=Task.Run(()=>dispatch.TryCopyAndOpen("review"));
            try {
                That(entered.Wait(TimeSpan.FromSeconds(5)),"Synthetic callback not reached");
                That(dispatch.TryCopyAndOpen("other")==ShareDispatchResult.Busy,"Concurrent click queued");
            } finally {release.Set();}
            That(first.GetAwaiter().GetResult()==ShareDispatchResult.CopiedAndOpened && copies==1 && opens==1,"Concurrent effects repeated");
            int attempts=0;
            var unexpected=new ShareDispatch(_=>{if(++attempts==1)throw new IOException("Synthetic unexpected error");},()=>{});
            try {unexpected.TryCopyAndOpen("review");throw new Exception("Unexpected error swallowed");}catch(IOException){}
            That(unexpected.TryCopyAndOpen("review")==ShareDispatchResult.CopiedAndOpened,"Guard remained stuck after callback exception");
        });
    }
}
