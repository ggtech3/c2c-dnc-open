using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using SerialSimulation;
using WindowsDnc;
class Reports : IProgress<DncProgress>
{
    public List<DncProgress> Items = new();
    public void Report(DncProgress p) => Items.Add(p);
}
class Program
{
    static int passed, failed;
    static SerialDncService service = new();
    static CncProfile P => CncProfile.BuiltIn[0] with { PortName="SIM-ONLY", LineDelayMilliseconds=0 };
    static FakeSerialPort Last => FakeSerialPort.Instances.Last();
    static void Assert(bool b, string message) { if(!b) throw new Exception(message); }
    static void Reset(Scenario? s=null) { FakeSerialPort.Next = s ?? new(); FakeSerialPort.Instances.Clear(); }
    static void Closed() => Assert(Last.Disposed && !Last.IsOpen,"Endpoint not disposed/closed");
    static void NoComplete(Reports r) => Assert(!r.Items.Any(p=>p.Message.StartsWith("Send complete") || p.Message.StartsWith("Receive complete")),"Unexpected success");
    static async Task Throws<T>(Func<Task> f) where T:Exception
    { try { await f(); } catch(T) { return; } throw new Exception("Expected "+typeof(T).Name); }
    static async Task Test(string name,Func<Task> f) { try { await f(); passed++; Console.WriteLine("PASS "+name); } catch(Exception e) { failed++; Console.WriteLine("FAIL "+name+": "+e); } }
    static Task Send(CncProfile p,string s,Reports r,CancellationToken t=default) => service.SendAsync(p,s,r,t);
    static Scenario Incoming(params string[] chunks)
    { var s=new Scenario(); foreach(var c in chunks)s.Incoming.Enqueue(Encoding.ASCII.GetBytes(c)); return s; }
    static async Task Main()
    {
        Console.WriteLine("SOURCE SIMULATION ONLY; no real port object is created. "+DateTime.UtcNow.ToString("O"));
        await Test("20 repeated sends preserve exact filtered ASCII bytes; no service transfer cap",async ()=>{
            for(int i=0;i<20;i++) {
                Reset(); var r=new Reports();
                await Send(P,"%\r\nO1000\nG01 X1 \tY2\0\r(ignore)\nG02 X3 (omit)\nM30\n%\n",r);
                Assert(Last.Output.SequenceEqual(Encoding.ASCII.GetBytes("%\r\nO1000\r\nG01X1Y2\r\nM30\r\n%\r\n")),"Byte mismatch");
                Assert(r.Items.Last().Percent==100 && r.Items.Last().BytesProcessed==Last.Output.Count,"Totals mismatch"); Closed();
            }
        });
        await Test("All 18 built-in profile settings mapped to fake endpoint", async ()=>{
            Assert(CncProfile.BuiltIn.Count==18,"Profile count");
            foreach(var original in CncProfile.BuiltIn) {
                Reset(); var p=original with {PortName="SIM-ONLY",LineDelayMilliseconds=0}; await Send(p,"M30",new());
                var f=Last;
                Assert(f.PortName==p.PortName && f.BaudRate==p.BaudRate && f.DataBits==p.DataBits && f.Parity==p.Parity && f.StopBits==p.StopBits && f.Handshake==p.Handshake,"Framing");
                Assert(f.RtsEnable==(p.Handshake is not Handshake.RequestToSend and not Handshake.RequestToSendXOnXOff && p.RtsEnable) && f.DtrEnable==p.DtrEnable,"Signals");
                Assert(f.NewLine==p.NewLine && f.ReadTimeout==500 && f.WriteTimeout==5000 && f.ReadBufferSize==4096 && f.WriteBufferSize==4096,"Configuration");
                Assert(f.Encoding.CodePage==Encoding.ASCII.CodePage && f.ParityReplace==(p.CheckParity?(byte)Math.Clamp(p.ParityReplaceCharacter,0,255):(byte)0),"Encoding/parity"); Closed();
            }
        });
        await Test("Percent framing, split CRLF, NUL/control filtering, preamble/trailer discarded",async ()=>{
            Reset(Incoming("noise","%","\r","\nO1\0 ","\tG1\u0011X2\r\n","M30\r\n%TRAILER"));
            var r=new Reports(); var result=await service.ReceiveAsync(P,r,default);
            Assert(result=="%\r\nO1G1X2\r\nM30\r\n%","Unexpected receive: "+result); Closed();
        });
        await Test("Nonframed idle timeout 1 second after data preserves spaces",async ()=>{
            Reset(Incoming("G1 X2\r","\nM30")); var watch=Stopwatch.StartNew();
            var result=await service.ReceiveAsync(P with {ReceivePercentFramed=false,ReceiveIdleTimeoutSeconds=1,RemoveWhitespace=false},new Reports(),default);
            Assert(result=="G1 X2\r\nM30" && watch.Elapsed.TotalSeconds>=1 && watch.Elapsed.TotalSeconds<4,"Idle behavior"); Closed();
        });
        await Test("Known behavior: incomplete percent frame completes on idle fallback",async ()=>{
            Reset(Incoming("%\r\nM30")); var r=new Reports();
            var result=await service.ReceiveAsync(P with {ReceiveIdleTimeoutSeconds=1},r,default);
            Assert(result=="%\r\nM30" && r.Items.Last().Message.StartsWith("Receive complete after"),"Behavior changed"); Closed();
        });
        await Test("No initial data does not trigger idle completion; cancellation closes",async ()=>{
            Reset(); var r=new Reports(); using var c=new CancellationTokenSource(150);
            await Throws<OperationCanceledException>(()=>service.ReceiveAsync(P with {ReceiveIdleTimeoutSeconds=0},r,c.Token)); NoComplete(r); Closed();
        });
        await Test("Cancelled partial receive closes and does not report complete",async ()=>{
            Reset(Incoming("%\nG1")); var r=new Reports(); using var c=new CancellationTokenSource(150);
            await Throws<OperationCanceledException>(()=>service.ReceiveAsync(P with {ReceiveIdleTimeoutSeconds=null},r,c.Token)); NoComplete(r); Closed();
            Assert(r.Items.Any(x=>x.ReceivedText?.Contains("G1")==true),"Partial preview missing");
        });
        await Test("Precancelled send emits no bytes (opens then closes)",async ()=>{
            Reset(); using var c=new CancellationTokenSource(); c.Cancel(); var r=new Reports();
            await Throws<OperationCanceledException>(()=>Send(P,"M30",r,c.Token)); Assert(Last.Output.Count==0,"Sent bytes"); NoComplete(r); Closed();
        });
        await Test("Cancel after first line stops later lines",async ()=>{
            using var c=new CancellationTokenSource(); int writes=0; Reset(new Scenario {AfterWrite=_=>{if(++writes==2)c.Cancel();}});
            var r=new Reports(); await Throws<OperationCanceledException>(()=>Send(P with {LineDelayMilliseconds=1},"A\nB\nC",r,c.Token));
            Assert(Encoding.ASCII.GetString(Last.Output.ToArray())=="A\r\n","Unexpected partial output"); NoComplete(r); Closed();
        });
        await Test("Open failure disposes endpoint and sends nothing",async ()=>{
            Reset(new Scenario {OpenError=new UnauthorizedAccessException("Simulated busy port")}); var r=new Reports();
            await Throws<UnauthorizedAccessException>(()=>Send(P,"M30",r)); Assert(Last.Output.Count==0,"Bytes emitted"); NoComplete(r); Closed();
        });
        await Test("Receive open failure disposes endpoint",async ()=>{
            Reset(new Scenario {OpenError=new IOException("Simulated missing port")}); var r=new Reports();
            await Throws<IOException>(()=>service.ReceiveAsync(P,r,default)); NoComplete(r); Closed();
        });
        await Test("Disconnect during second line exposes partial bytes without automatic retry",async ()=>{
            Reset(new Scenario {FailWriteAt=3,PartialWriteBytes=1,DisconnectOnWrite=true}); var r=new Reports();
            await Throws<IOException>(()=>Send(P,"AA\nBB\nCC",r));
            Assert(Encoding.ASCII.GetString(Last.Output.ToArray())=="AA\r\nB" && FakeSerialPort.Instances.Count==1,"Unexpected retransmission"); NoComplete(r); Closed();
        });
        await Test("Injected write timeout propagates, no retry or success",async ()=>{
            Reset(new Scenario {FailWriteAt=1,WriteError=new TimeoutException("Simulated 5000ms write timeout")}); var r=new Reports();
            await Throws<TimeoutException>(()=>Send(P,"M30",r)); NoComplete(r); Closed();
        });
        await Test("Receive disconnected endpoint propagates failure",async ()=>{
            Reset(new Scenario {ReadError=new IOException("Simulated disconnect")}); var r=new Reports();
            await Throws<IOException>(()=>service.ReceiveAsync(P,r,default)); NoComplete(r); Closed();
        });
        await Test("Real algorithm drain timeout after 10 seconds",async ()=>{
            Reset(new Scenario {PersistentQueued=1}); var r=new Reports(); var watch=Stopwatch.StartNew();
            await Throws<TimeoutException>(()=>Send(P,"M30",r)); Assert(watch.Elapsed.TotalSeconds>=10 && watch.Elapsed.TotalSeconds<15,"Drain timeout duration"); NoComplete(r); Closed();
        });
        await Test("Cancellation while queue is draining",async ()=>{
            Reset(new Scenario {PersistentQueued=1}); var r=new Reports(); using var c=new CancellationTokenSource(150);
            await Throws<OperationCanceledException>(()=>Send(P,"M30",r,c.Token)); NoComplete(r); Closed();
        });
        await Test("Known behavior: repeated user requests are not deduplicated at service layer",async ()=>{
            Reset(); await Send(P,"M30",new()); await Send(P,"M30",new());
            Assert(FakeSerialPort.Instances.Count==2 && FakeSerialPort.Instances.All(f=>Encoding.ASCII.GetString(f.Output.ToArray())=="M30\r\n"),"Unexpected dedup");
        });
        await Test("Serial error count and unavailable modem signals reported",async ()=>{
            Reset(new Scenario {SignalErrors=true,AfterWrite=f=>f.RaiseError()}); var r=new Reports(); await Send(P,"M30",r);
            Assert(r.Items.Last().SerialErrors==2 && r.Items.Last().CtsActive==false && r.Items.Last().DsrActive==false,"Error/signal reporting"); Closed();
        });
        await Test("Fully filtered source rejects before opening",async ()=>{
            Reset(); await Throws<InvalidOperationException>(()=>Send(P,"(comment)",new())); Assert(FakeSerialPort.Instances.Count==0,"Port created");
        });
        await Test("Known behavior: empty source transmits one newline",async ()=>{
            Reset(); await Send(P,"",new()); Assert(Encoding.ASCII.GetString(Last.Output.ToArray())=="\r\n","Empty behavior changed"); Closed();
        });
        await Test("ASCII replacement and newline normalization match estimate",async ()=>{
            Reset(); var p=P with {RemoveWhitespace=false,OmitLinesContainingParenthesis=false};
            var estimate=SerialDncService.CalculateSendEstimate(p,"G1 Xé\nM30\r"); await Send(p,"G1 Xé\nM30\r",new());
            Assert(Encoding.ASCII.GetString(Last.Output.ToArray())=="G1 X?\r\nM30\r\n" && estimate.Bytes==Last.Output.Count && estimate.Lines==2,"ASCII/estimate mismatch"); Closed();
        });
        Console.WriteLine($"TOTAL {passed} passed, {failed} failed");
        Console.WriteLine("LIMITS: injected faults model behavior, not real drivers/timing/flow control. No packaged app, hardware, live CNC, electrical or end-to-end delivery validation.");
        Environment.ExitCode=failed==0?0:1;
    }
}
