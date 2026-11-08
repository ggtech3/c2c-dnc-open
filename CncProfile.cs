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
    public static IReadOnlyList<CncProfile> BuiltIn { get; } =
    [
        new(
            "emco-et342-siemens-810t",
            true,
            "EMCO ET342 / Siemens 810T",
            "COM7",
            1200,
            7,
            Parity.Even,
            StopBits.Two,
            Handshake.None,
            true,
            true,
            "\r\n",
            150,
            true,
            5,
            true,
            true,
            true,
            OmitLinesContainingParenthesis: true,
            RemoveReceiveControlCharacters: true,
            CheckParity: true,
            ParityReplaceCharacter: 35,
            CompatibilityRevision: 1),
        new(
            "yasnac-mx3",
            true,
            "Yasnac MX-3",
            "COM6",
            9600,
            7,
            Parity.Even,
            StopBits.Two,
            Handshake.XOnXOff,
            true,
            true,
            "\r\n",
            50,
            false,
            5,
            true,
            false,
            false),
        new(
            "haas-classic-control-rs232",
            true,
            "Haas Classic Control (RS-232)",
            "COM1",
            9600,
            7,
            Parity.Even,
            StopBits.One,
            Handshake.XOnXOff,
            true,
            true,
            "\r\n",
            50,
            true,
            null,
            true,
            false,
            false,
            "Field-tested successfully on an older Haas Classic Control. Verify Haas Settings 11–14, 25, and 37 because machine configurations can differ.",
            "https://www.haascnc.com/content/dam/haascnc/en/service/manual/operator/english---mill-operator%27s-manual---2009.pdf"),
        new(
            "fanuc-0i-16i-18i-21i-rs232",
            true,
            "FANUC 0i / 16i / 18i / 21i (RS-232)",
            "COM1",
            9600,
            7,
            Parity.Even,
            StopBits.Two,
            Handshake.XOnXOff,
            true,
            true,
            "\r\n",
            20,
            true,
            null,
            true,
            false,
            false,
            "Starter preset: verify I/O channel and parameters 0101–0103/0121–0123.",
            "https://www.drivesul.com.br/template/imagens/manuais/manuais-fanuc/fanuc-series-0i-model-f/FANUC%20Series%200i%20MODEL%20F%20-%20PARAMETER%20MANUAL%20.pdf"),
        new(
            "siemens-802d-sl-v24",
            true,
            "Siemens SINUMERIK 802D sl (V24)",
            "COM1",
            19200,
            8,
            Parity.None,
            StopBits.One,
            Handshake.RequestToSend,
            true,
            true,
            "\r\n",
            20,
            false,
            5,
            true,
            false,
            false,
            "Starter preset: select punched-tape/text format and verify the V24 screen.",
            "https://support.industry.siemens.com/cs/attachments/104946900/802Dsl_BE_0505_en.pdf"),
        new(
            "fagor-8040-nclink",
            true,
            "Fagor 8040 (NC-Link starting point)",
            "COM1",
            9600,
            7,
            Parity.Even,
            StopBits.Two,
            Handshake.RequestToSend,
            true,
            true,
            "\n",
            30,
            false,
            5,
            true,
            false,
            false,
            "Starting point only: match the CNC serial parameters and place the control in Receive DNC / Waiting for Data before sending.",
            "https://industrialmonitordirect.com/blogs/knowledgebase/fagor-8040-dnc-configuring-onecnc-nc-link-rs-232-transfer"),
        new(
            "fagor-8055-generic-115200",
            true,
            "Fagor 8055 (Generic RS-232, 115200)",
            "COM1",
            115200,
            8,
            Parity.None,
            StopBits.One,
            Handshake.XOnXOff,
            true,
            true,
            "\n",
            0,
            false,
            5,
            true,
            false,
            false,
            "Starting point: use Serial 2 with P000=11, P001=1, P002=0, P003=0, P004=0, P008=0, and P010=ON; then restart the control.",
            "https://dmscncrouters.com/wp-content/uploads/2016/05/Fagor-CNC-8055-Installation-Manual-English.pdf"),
        new(
            "fagor-8055-generic-9600",
            true,
            "Fagor 8055 (Generic RS-232, 9600)",
            "COM1",
            9600,
            8,
            Parity.None,
            StopBits.One,
            Handshake.XOnXOff,
            true,
            true,
            "\n",
            0,
            false,
            5,
            true,
            false,
            false,
            "Slower starting point: use Serial 2 with P000=7, P001=1, P002=0, P003=0, P004=0, P008=0, and P010=ON; then restart the control.",
            "https://dmscncrouters.com/wp-content/uploads/2016/05/Fagor-CNC-8055-Installation-Manual-English.pdf"),
        new(
            "mazak-m32-t32-eia",
            true,
            "Mazak M32 / T32 (EIA/ISO starting point)",
            "COM1", 4800, 7, Parity.Even, StopBits.Two, Handshake.XOnXOff,
            true, true, "\r\n", 20, true, 5, true, false, false,
            "EIA/ISO text programs only—not Mazatrol conversational files. Verify the USER parameter page, DC control, and cable.",
            "https://www.aggsoft.com/cnc-dnc/mazak.htm"),
        new(
            "mazak-fusion-640-eia",
            true,
            "Mazak Fusion 640M (EIA/ISO starting point)",
            "COM1", 9600, 7, Parity.Even, StopBits.Two, Handshake.XOnXOff,
            true, true, "\r\n", 20, true, 5, true, false, false,
            "EIA/ISO text programs only. Select TAPE/ASCII format and verify the control's TAP parameters and DC control.",
            "https://www.aggsoft.com/cnc-dnc/mazak.htm"),
        new(
            "mitsubishi-meldas-m50-m60",
            true,
            "Mitsubishi MELDAS M50 / M60 (starting point)",
            "COM1", 4800, 7, Parity.Even, StopBits.One, Handshake.XOnXOff,
            true, true, "\n", 20, true, 5, true, false, false,
            "Verify the active IN/OUT device, port, DC1/DC3 control, ISO output, and the 910x/920x parameter groups.",
            "https://www.aggsoft.com/cnc-dnc/mitsubishi.htm"),
        new(
            "okuma-osp-5000-7000",
            true,
            "Okuma OSP 5000 / 7000 (starting point)",
            "COM1", 4800, 7, Parity.Even, StopBits.One, Handshake.XOnXOff,
            true, true, "\n", 20, true, 5, true, false, false,
            "Use ISO and DC-code flow control; verify CN0, read/punch device numbers, delimiter, EOB, and optional parameter bits.",
            "https://www.cncapplications.co.uk/support/Okuma_OSP-5000.pdf"),
        new(
            "heidenhain-tnc-151-155-351-355-ext",
            true,
            "HEIDENHAIN TNC 151/155/351/355 (EXT ASCII)",
            "COM1", 9600, 7, Parity.Even, StopBits.One, Handshake.XOnXOff,
            true, true, "\r\n", 20, false, 5, true, false, false,
            "Select EXT standard transfer with MP222/MP5020=168. C2C does not implement HEIDENHAIN FE or LSV2 block protocols.",
            "https://product.heidenhain.de/JPBC/image/FILEBASE_PUBLIC/599456-02-A-02.pdf"),
        new(
            "fadal-cnc-88-88hs",
            true,
            "Fadal CNC 88 / 88HS (starting point)",
            "COM1", 9600, 7, Parity.Even, StopBits.One, Handshake.XOnXOff,
            true, true, "\r\n", 20, false, 5, true, false, false,
            "The 7E1 framing and XON/XOFF are fixed; set baud with SETP or CD,8 and verify the null-modem cable.",
            "https://support.shopfloorautomations.com/portal/en/kb/articles/fadal-cnc-88hs-rs-232-parameters"),
        new(
            "brother-tc-32a-ascii",
            true,
            "Brother TC-32A (ASCII starting point)",
            "COM1", 9600, 7, Parity.None, StopBits.One, Handshake.None,
            true, true, "\r\n", 20, false, 5, true, false, false,
            "Select port/object 0 and ASCII protocol. Some programs use %% framing; verify DC codes and the exact Brother control generation.",
            "https://www.aggsoft.com/cnc-dnc/brother.htm"),
        new(
            "siemens-810d-840d-pcin-ascii",
            true,
            "Siemens SINUMERIK 810D / 840D (PCIN ASCII)",
            "COM1", 19200, 8, Parity.None, StopBits.One, Handshake.XOnXOff,
            true, true, "\n", 20, false, 5, true, false, false,
            "Use PCIN ASCII/punched-tape format, not archive protocol. Match the V24 interface and select XON/XOFF.",
            "https://support.industry.siemens.com/cs/attachments/108838303/INC_0405_en.pdf"),
        new(
            "yasnac-i80",
            true,
            "Yasnac i80 (starting point)",
            "COM1", 9600, 7, Parity.Even, StopBits.Two, Handshake.XOnXOff,
            true, true, "\r\n", 30, false, 5, true, false, false,
            "Set the selected RS232C IN and OUT columns to 9600, 2 stop bits, even parity and XON/XOFF; RTS depends on cable wiring.",
            "https://www.aggsoft.com/cnc-dnc/yaskawa.htm"),
        new(
            "fanuc-6-10-11-12-15-legacy",
            true,
            "FANUC 6 / 10 / 11 / 12 / 15 (legacy starting point)",
            "COM1", 4800, 7, Parity.Even, StopBits.Two, Handshake.XOnXOff,
            true, true, "\r\n", 30, true, 5, true, false, false,
            "Legacy FANUC starting point only. Verify the active reader/punch channel, ISO code, baud and I/O parameters before transfer.",
            "https://www.aggsoft.com/cnc-dnc/fanuc.htm")
    ];

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
