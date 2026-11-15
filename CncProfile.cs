using System.IO.Ports;
using System.Text.Json.Serialization;

namespace WindowsDnc;

public sealed record CncProfile(
    string Id,
    bool IsBuiltIn,
    string Name,
    string PortName,
    int BaudRate,
    int DataBits,
    Parity Parity,
    StopBits StopBits,
    Handshake Handshake,
    bool DtrEnable,
    bool RtsEnable,
    string NewLine,
    int LineDelayMilliseconds,
    bool ReceivePercentFramed,
    int? ReceiveIdleTimeoutSeconds,
    bool StripNulls,
    bool RemoveWhitespace,
    bool OmitParenthesisLines,
    string Guidance = "",
    string SourceUrl = "",
    bool OmitLinesContainingParenthesis = false,
    bool RemoveReceiveControlCharacters = false,
    bool CheckParity = false,
    int ParityReplaceCharacter = 0,
    int CompatibilityRevision = 0)
{
    public static IReadOnlyList<CncProfile> BuiltIn { get; } = Array.Empty<CncProfile>();

    public static CncProfile NewBlank() => new(
        Guid.NewGuid().ToString("N"), false, "New profile - Not confirmed", "COM1",
        9600, 8, Parity.None, StopBits.One, Handshake.None,
        false, false, "\r\n", 0, false, 5, true, false, false,
        "Not confirmed. Configure every setting for your own controller, cable and adapter before use.");

    [JsonIgnore]
    public string Summary =>
        $"{PortName}  •  {BaudRate} baud  •  {DataBits}{ParityLetter}{StopBitsValue}  •  " +
        $"{HandshakeName}\r\n" +
        $"{(DtrEnable ? "DTR on" : "DTR off")}  •  {(RtsEnable ? "RTS on" : "RTS off")}  •  " +
        $"{LineEndingName}  •  {LineDelayMilliseconds} ms line delay\r\n" +
        ReceiveSummary +
        (string.IsNullOrWhiteSpace(Guidance) ? string.Empty : $"\r\n{Guidance}");

    private string ReceiveSummary =>
        (ReceivePercentFramed
            ? $"Receive: starts/ends with %{(ReceiveIdleTimeoutSeconds is int fallback ? $"; {fallback}s idle fallback" : string.Empty)}"
            : $"Receive: finish after {ReceiveIdleTimeoutSeconds} seconds idle") +
        $"  •  {(StripNulls ? "strip NULs" : "keep NULs")}" +
        $"  •  {(RemoveWhitespace ? "remove spaces/tabs" : "preserve whitespace")}" +
        (RemoveReceiveControlCharacters ? "  •  strip control chars" : string.Empty) +
        (CheckParity ? $"  •  parity errors→ASCII {ParityReplaceCharacter}" : string.Empty) +
        (OmitLinesContainingParenthesis
            ? "\r\nSend: omit lines containing ("
            : OmitParenthesisLines ? "\r\nSend: omit lines beginning with (" : string.Empty);

    private string ParityLetter => Parity switch
    {
        Parity.Even => "E",
        Parity.Odd => "O",
        Parity.Mark => "M",
        Parity.Space => "S",
        _ => "N"
    };

    private string StopBitsValue => StopBits switch
    {
        StopBits.Two => "2",
        StopBits.OnePointFive => "1.5",
        _ => "1"
    };

    private string HandshakeName => Handshake switch
    {
        Handshake.XOnXOff => "XON/XOFF",
        Handshake.RequestToSend => "RTS/CTS",
        Handshake.RequestToSendXOnXOff => "RTS/CTS + XON/XOFF",
        _ => "No flow control"
    };

    private string LineEndingName => NewLine switch
    {
        "\n" => "LF",
        "\r" => "CR",
        _ => "CR/LF"
    };
}
