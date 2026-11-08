using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace WindowsDnc;

public sealed record DncProgress(
    string Message,
    int? Percent = null,
    string? ReceivedText = null,
    TimeSpan? EstimatedRemaining = null,
    TimeSpan? Elapsed = null,
    int? LinesProcessed = null,
    int? TotalLines = null,
    long? BytesProcessed = null,
    long? TotalBytes = null,
    double? CharactersPerSecond = null,
    string? RawChunk = null,
    int? SerialErrors = null,
    bool? RxActive = null,
    bool? TxActive = null,
    bool? RtsActive = null,
    bool? CtsActive = null,
    bool? DtrActive = null,
    bool? DsrActive = null);

public sealed record DncSendEstimate(
    int Lines,
    long Bytes,
    TimeSpan Duration);

public sealed class SerialDncService
{
    public static DncSendEstimate CalculateSendEstimate(CncProfile profile, string source)
    {
        IReadOnlyList<string> lines = DncTextProcessor.PrepareForSend(source, profile);
        long totalBytes = lines.Sum(line =>
            (long)Encoding.ASCII.GetByteCount(line + profile.NewLine));
        TimeSpan duration = EstimateRemaining(
            totalBytes,
            Math.Max(0, lines.Count - 1),
            profile,
            GetBitsPerCharacter(profile));

        return new DncSendEstimate(lines.Count, totalBytes, duration);
    }

    public async Task SendAsync(
        CncProfile profile,
        string source,
        IProgress<DncProgress> progress,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> lines = DncTextProcessor.PrepareForSend(source, profile);
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("There are no lines to send after applying the profile filters.");
        }

        using SerialPort port = CreatePort(profile);
        int serialErrors = 0;
        port.ErrorReceived += (_, _) => Interlocked.Increment(ref serialErrors);
        port.Open();
        int[] payloadBytes = lines
            .Select(line => Encoding.ASCII.GetByteCount(line + profile.NewLine))
            .ToArray();
        long[] suffixBytes = BuildSuffixTotals(payloadBytes);
        double bitsPerCharacter = GetBitsPerCharacter(profile);
        long totalBytes = suffixBytes[0];
        long queuedBytes = 0;
        var transferWatch = Stopwatch.StartNew();
        TimeSpan initialEstimate = EstimateRemaining(
            suffixBytes[0],
            Math.Max(0, lines.Count - 1),
            profile,
            bitsPerCharacter);
        progress.Report(new(
            $"Opened {profile.PortName}; sending {lines.Count:N0} lines.",
            0,
            EstimatedRemaining: initialEstimate,
            Elapsed: transferWatch.Elapsed,
            LinesProcessed: 0,
            TotalLines: lines.Count,
            BytesProcessed: 0,
            TotalBytes: totalBytes,
            SerialErrors: serialErrors,
            TxActive: true,
            RtsActive: port.RtsEnable,
            CtsActive: SafeSignal(() => port.CtsHolding),
            DtrActive: port.DtrEnable,
            DsrActive: SafeSignal(() => port.DsrHolding)));

        try
        {
            for (int index = 0; index < lines.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                port.Write(lines[index]);
                port.Write(profile.NewLine);
                queuedBytes += payloadBytes[index];

                int percent = (int)Math.Round((index + 1) * 100d / lines.Count);
                long remainingBytes = port.BytesToWrite + suffixBytes[index + 1];
                int remainingDelays = Math.Max(0, lines.Count - index - 1);
                TimeSpan remaining = EstimateRemaining(
                    remainingBytes,
                    remainingDelays,
                    profile,
                    bitsPerCharacter);
                progress.Report(new(
                    $"Sending line {index + 1:N0} of {lines.Count:N0}",
                    percent,
                    EstimatedRemaining: remaining,
                    Elapsed: transferWatch.Elapsed,
                    LinesProcessed: index + 1,
                    TotalLines: lines.Count,
                    BytesProcessed: queuedBytes,
                    TotalBytes: totalBytes,
                    CharactersPerSecond: queuedBytes / Math.Max(0.001, transferWatch.Elapsed.TotalSeconds),
                    RawChunk: lines[index] + profile.NewLine,
                    SerialErrors: serialErrors,
                    TxActive: true,
                    RtsActive: port.RtsEnable,
                    CtsActive: SafeSignal(() => port.CtsHolding),
                    DtrActive: port.DtrEnable,
                    DsrActive: SafeSignal(() => port.DsrHolding)));

                if (index < lines.Count - 1 && profile.LineDelayMilliseconds > 0)
                {
                    await Task.Delay(profile.LineDelayMilliseconds, cancellationToken);
                }
            }

            // Wait for the driver's transmit queue to drain before closing the port.
            var drainWatch = Stopwatch.StartNew();
            while (port.BytesToWrite > 0 && drainWatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                cancellationToken.ThrowIfCancellationRequested();
                TimeSpan remaining = EstimateRemaining(
                    port.BytesToWrite,
                    0,
                    profile,
                    bitsPerCharacter);
                progress.Report(new(
                    "Finishing serial transmission",
                    99,
                    EstimatedRemaining: remaining,
                    Elapsed: transferWatch.Elapsed,
                    LinesProcessed: lines.Count,
                    TotalLines: lines.Count,
                    BytesProcessed: totalBytes - port.BytesToWrite,
                    TotalBytes: totalBytes,
                    CharactersPerSecond: (totalBytes - port.BytesToWrite) /
                        Math.Max(0.001, transferWatch.Elapsed.TotalSeconds),
                    SerialErrors: serialErrors,
                    TxActive: true,
                    RtsActive: port.RtsEnable,
                    CtsActive: SafeSignal(() => port.CtsHolding),
                    DtrActive: port.DtrEnable,
                    DsrActive: SafeSignal(() => port.DsrHolding)));
                await Task.Delay(100, cancellationToken);
            }

            if (port.BytesToWrite > 0)
            {
                throw new TimeoutException("The serial transmit queue did not drain within 10 seconds.");
            }

            progress.Report(new(
                $"Send complete: {lines.Count:N0} lines.",
                100,
                Elapsed: transferWatch.Elapsed,
                LinesProcessed: lines.Count,
                TotalLines: lines.Count,
                BytesProcessed: totalBytes,
                TotalBytes: totalBytes,
                CharactersPerSecond: totalBytes / Math.Max(0.001, transferWatch.Elapsed.TotalSeconds),
                SerialErrors: serialErrors,
                TxActive: false,
                RtsActive: port.RtsEnable,
                CtsActive: SafeSignal(() => port.CtsHolding),
                DtrActive: port.DtrEnable,
                DsrActive: SafeSignal(() => port.DsrHolding)));
        }
        finally
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }
    }

    public async Task<string> ReceiveAsync(
        CncProfile profile,
        IProgress<DncProgress> progress,
        CancellationToken cancellationToken)
    {
        using SerialPort port = CreatePort(profile);
        int serialErrors = 0;
        port.ErrorReceived += (_, _) => Interlocked.Increment(ref serialErrors);
        port.Open();

        var received = new StringBuilder();
        bool frameStarted = !profile.ReceivePercentFramed;
        bool hasReceivedData = false;
        DateTime lastDataUtc = DateTime.UtcNow;
        var receiveWatch = Stopwatch.StartNew();
        long receivedBytes = 0;

        progress.Report(new(
            profile.ReceivePercentFramed
                ? $"Opened {profile.PortName}; waiting for the first % marker."
                : $"Opened {profile.PortName}; waiting for data.",
            SerialErrors: serialErrors,
            RxActive: false,
            RtsActive: port.RtsEnable,
            CtsActive: SafeSignal(() => port.CtsHolding),
            DtrActive: port.DtrEnable,
            DsrActive: SafeSignal(() => port.DsrHolding)));

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int available = port.BytesToRead;
                if (available > 0)
                {
                    byte[] buffer = new byte[Math.Min(available, 4096)];
                    int count = port.Read(buffer, 0, buffer.Length);
                    receivedBytes += count;
                    string rawChunk = Encoding.ASCII.GetString(buffer, 0, count);
                    string chunk = rawChunk;
                    if (profile.StripNulls)
                    {
                        chunk = chunk.Replace("\0", string.Empty);
                    }

                    if (chunk.Length > 0)
                    {
                        hasReceivedData = true;
                        lastDataUtc = DateTime.UtcNow;

                        foreach (char character in chunk)
                        {
                            if (!frameStarted)
                            {
                                if (character != '%')
                                {
                                    continue;
                                }
                                frameStarted = true;
                                received.Append(character);
                                progress.Report(new(
                                    "Start marker received; receiving program.",
                                    Elapsed: receiveWatch.Elapsed,
                                    SerialErrors: serialErrors,
                                    RxActive: true));
                                continue;
                            }

                            received.Append(character);
                            if (profile.ReceivePercentFramed &&
                                character == '%' &&
                                received.Length > 1)
                            {
                                string framedResult = DncTextProcessor.CleanReceived(received.ToString(), profile);
                                progress.Report(new(
                                    $"Receive complete: {framedResult.Length:N0} characters.",
                                    100,
                                    framedResult,
                                    Elapsed: receiveWatch.Elapsed,
                                    BytesProcessed: receivedBytes,
                                    CharactersPerSecond: receivedBytes /
                                        Math.Max(0.001, receiveWatch.Elapsed.TotalSeconds),
                                    RawChunk: rawChunk,
                                    SerialErrors: serialErrors,
                                    RxActive: false,
                                    RtsActive: port.RtsEnable,
                                    CtsActive: SafeSignal(() => port.CtsHolding),
                                    DtrActive: port.DtrEnable,
                                    DsrActive: SafeSignal(() => port.DsrHolding)));
                                return framedResult;
                            }
                        }

                        string preview = DncTextProcessor.CleanReceived(received.ToString(), profile);
                        progress.Report(new(
                            $"Receiving: {preview.Length:N0} characters.",
                            null,
                            preview,
                            Elapsed: receiveWatch.Elapsed,
                            BytesProcessed: receivedBytes,
                            CharactersPerSecond: receivedBytes /
                                Math.Max(0.001, receiveWatch.Elapsed.TotalSeconds),
                            RawChunk: rawChunk,
                            SerialErrors: serialErrors,
                            RxActive: true,
                            RtsActive: port.RtsEnable,
                            CtsActive: SafeSignal(() => port.CtsHolding),
                            DtrActive: port.DtrEnable,
                            DsrActive: SafeSignal(() => port.DsrHolding)));
                    }
                }
                else if (hasReceivedData &&
                         profile.ReceiveIdleTimeoutSeconds is int idleSeconds &&
                         DateTime.UtcNow - lastDataUtc >= TimeSpan.FromSeconds(idleSeconds))
                {
                    string idleResult = DncTextProcessor.CleanReceived(received.ToString(), profile);
                    progress.Report(new(
                        $"Receive complete after {idleSeconds} seconds idle: {idleResult.Length:N0} characters.",
                        100,
                        idleResult,
                        Elapsed: receiveWatch.Elapsed,
                        BytesProcessed: receivedBytes,
                        CharactersPerSecond: receivedBytes /
                            Math.Max(0.001, receiveWatch.Elapsed.TotalSeconds),
                        SerialErrors: serialErrors,
                        RxActive: false,
                        RtsActive: port.RtsEnable,
                        CtsActive: SafeSignal(() => port.CtsHolding),
                        DtrActive: port.DtrEnable,
                        DsrActive: SafeSignal(() => port.DsrHolding)));
                    return idleResult;
                }

                await Task.Delay(20, cancellationToken);
            }
        }
        finally
        {
            if (port.IsOpen)
            {
                port.Close();
            }
        }
    }

    private static SerialPort CreatePort(CncProfile profile) => new()
    {
        PortName = profile.PortName,
        BaudRate = profile.BaudRate,
        DataBits = profile.DataBits,
        Parity = profile.Parity,
        StopBits = profile.StopBits,
        Handshake = profile.Handshake,
        DtrEnable = profile.DtrEnable,
        RtsEnable = profile.Handshake is not Handshake.RequestToSend
            and not Handshake.RequestToSendXOnXOff && profile.RtsEnable,
        NewLine = profile.NewLine,
        Encoding = Encoding.ASCII,
        ReadTimeout = 500,
        WriteTimeout = 5000,
        ReadBufferSize = 4096,
        WriteBufferSize = 4096
        ,ParityReplace = profile.CheckParity
            ? (byte)Math.Clamp(profile.ParityReplaceCharacter, 0, 255)
            : (byte)0
    };

    private static bool SafeSignal(Func<bool> read)
    {
        try { return read(); }
        catch (InvalidOperationException) { return false; }
        catch (IOException) { return false; }
    }

    private static long[] BuildSuffixTotals(IReadOnlyList<int> values)
    {
        var suffix = new long[values.Count + 1];
        for (int index = values.Count - 1; index >= 0; index--)
        {
            suffix[index] = suffix[index + 1] + values[index];
        }
        return suffix;
    }

    private static TimeSpan EstimateRemaining(
        long remainingBytes,
        int remainingDelays,
        CncProfile profile,
        double bitsPerCharacter)
    {
        double wireMilliseconds =
            remainingBytes * bitsPerCharacter * 1000d / profile.BaudRate;
        double delayMilliseconds =
            remainingDelays * (double)profile.LineDelayMilliseconds;

        // Serial output and line pacing overlap while bytes drain from the driver queue.
        return TimeSpan.FromMilliseconds(Math.Max(wireMilliseconds, delayMilliseconds));
    }

    private static double GetBitsPerCharacter(CncProfile profile)
    {
        double parityBits = profile.Parity == Parity.None ? 0 : 1;
        double stopBits = profile.StopBits switch
        {
            StopBits.OnePointFive => 1.5,
            StopBits.Two => 2,
            _ => 1
        };
        return 1 + profile.DataBits + parityBits + stopBits;
    }
}
