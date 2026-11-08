using System.IO.Ports;
using System.Text;
namespace SerialSimulation;
public sealed class Scenario
{
    public Queue<byte[]> Incoming { get; } = new();
    public Exception? OpenError, ReadError, WriteError;
    public int FailWriteAt = int.MaxValue, PartialWriteBytes, PersistentQueued;
    public bool DisconnectOnWrite, SignalErrors;
    public Action<FakeSerialPort>? AfterWrite;
}
public sealed class FakeSerialPort : IDisposable
{
    public static Scenario Next = new();
    public static List<FakeSerialPort> Instances = new();
    private readonly Scenario scenario;
    private int writes;
    public FakeSerialPort() { scenario = Next; Instances.Add(this); }
    public event EventHandler? ErrorReceived;
    public string PortName {get;set;} = "";
    public int BaudRate {get;set;}
    public int DataBits {get;set;}
    public Parity Parity {get;set;}
    public StopBits StopBits {get;set;}
    public Handshake Handshake {get;set;}
    public bool DtrEnable {get;set;}
    public bool RtsEnable {get;set;}
    public string NewLine {get;set;} = "";
    public Encoding Encoding {get;set;} = Encoding.ASCII;
    public int ReadTimeout {get;set;}
    public int WriteTimeout {get;set;}
    public int ReadBufferSize {get;set;}
    public int WriteBufferSize {get;set;}
    public byte ParityReplace {get;set;}
    public bool IsOpen {get;private set;}
    public bool Disposed {get;private set;}
    public int OpenCalls {get;private set;}
    public List<byte> Output {get;} = new();
    public bool CtsHolding => scenario.SignalErrors ? throw new IOException("simulated signal error") : true;
    public bool DsrHolding => CtsHolding;
    public int BytesToWrite { get { Check(); return scenario.PersistentQueued; } }
    public int BytesToRead { get { Check(); if(scenario.ReadError != null) throw scenario.ReadError; return scenario.Incoming.TryPeek(out var b) ? b.Length : 0; } }
    public void Open()
    {
        OpenCalls++;
        if(!PortName.StartsWith("SIM-", StringComparison.Ordinal)) throw new InvalidOperationException("Only SIM names allowed");
        if(scenario.OpenError != null) throw scenario.OpenError;
        IsOpen = true;
    }
    public void Close() { IsOpen = false; }
    public void Dispose() { IsOpen = false; Disposed = true; }
    private void Check() { if(!IsOpen) throw new IOException("Simulated disconnected endpoint"); }
    public void Write(string text)
    {
        Check(); writes++;
        var bytes = Encoding.GetBytes(text);
        if(writes == scenario.FailWriteAt)
        {
            Output.AddRange(bytes.Take(scenario.PartialWriteBytes));
            if(scenario.DisconnectOnWrite) IsOpen = false;
            throw scenario.WriteError ?? new IOException("Simulated write failure");
        }
        Output.AddRange(bytes);
        scenario.AfterWrite?.Invoke(this);
    }
    public int Read(byte[] buffer,int offset,int count)
    {
        Check();
        var bytes = scenario.Incoming.Dequeue();
        if(bytes.Length > count) throw new InvalidOperationException("Harness chunks must fit read buffer");
        Array.Copy(bytes,0,buffer,offset,bytes.Length);
        return bytes.Length;
    }
    public void RaiseError() => ErrorReceived?.Invoke(this, EventArgs.Empty);
}
