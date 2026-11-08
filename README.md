# C2C DNC

## Repository status and development

Private owner review only. No open-source license has been selected and no public release is authorized by this snapshot. Read [licensing status](LICENSE-STATUS.md), [contributing](CONTRIBUTING.md), [security](SECURITY.md), [governance](GOVERNANCE.md) and the [Spec Kit workflow](docs/SPEC-KIT.md).

The source history starts fresh from a sanitized preview snapshot. Private licensing tools, prior commercial Git history, customer data, runtime state and packaged binaries are excluded. Automated serial checks use a fake endpoint; see [simulation limits](Tests/SerialSimulation/README.md). A passing build or simulation is not machine acceptance.


A Windows 11 RS-232 sender/receiver with 18 built-in CNC profiles:

This separate Open Preview is based on 0.9.8/v28. It opens directly, with no
registration, activation, customer identity, purchase step, or transfer limit.
Existing trial/paid releases and their data are not changed. This is a private
owner-review build, not a public release or an open-source licensing grant.

- **EMCO ET342 / Siemens 810T** — COM7, 1200 baud, 7E2, no flow
  control, DTR/RTS on, CR/LF, 150 ms line delay. Receive is framed by
  `%` markers. NUL bytes and horizontal whitespace are removed. On send,
  lines containing `(` are omitted. Receive also has a five-second idle
  fallback, removes control characters below ASCII 32 while preserving line
  endings, and replaces parity-error characters with `#` (ASCII 35).
- **Yasnac MX-3** — COM6, 9600 baud, 7E2, XON/XOFF, DTR/RTS on, CR/LF,
  50 ms line delay. Receive completes after five seconds without data.
  NUL bytes are removed and other whitespace is preserved.
- **Haas Classic Control (RS-232)** — historically field-tested in the baseline edition on an older
  Haas machine at 9600 baud, 7E1, and XON/XOFF. Confirm Haas Settings 11–14,
  25, and 37 because individual machine configurations can differ.
- **FANUC 0i / 16i / 18i / 21i (RS-232)** — a 9600 baud, 7E2, XON/XOFF
  starting point. Confirm the selected I/O channel and its 0101–0103 or
  0121–0123 parameter group.
- **Siemens SINUMERIK 802D sl (V24)** — the documented 19200 baud, 8N1,
  RTS/CTS V24 starting point. Select punched-tape/text format on the control.
- **Fagor 8040 (NC-Link starting point)** — 9600 baud, 7E2, RTS/CTS and LF.
  Put the control in Receive DNC / Waiting for Data before sending and verify
  every controller and cable setting.
- **Fagor 8055 (Generic RS-232, 115200)** — 115200 baud, 8N1, XON/XOFF and
  LF for the generic-peripheral protocol (`P004=0`).
- **Fagor 8055 (Generic RS-232, 9600)** — a slower 8N1, XON/XOFF and LF
  alternative for existing installations or older serial hardware.
- **Mazak M32/T32 and Fusion 640M** — EIA/ISO text starting points. Mazatrol
  conversational transfer is a different protocol and is not supported.
- **Mitsubishi MELDAS M50/M60** — 4800 baud, 7E1, XON/XOFF and LF.
- **Okuma OSP 5000/7000** — 4800 baud, 7E1, XON/XOFF and LF.
- **HEIDENHAIN TNC 151/155/351/355** — EXT standard ASCII at 9600 baud,
  7E1 and XON/XOFF. FE and LSV2 protocols are not supported.
- **Fadal CNC 88/88HS** — 9600 baud, fixed 7E1 and XON/XOFF.
- **Brother TC-32A** — ASCII starting point at 9600 baud, 7N1.
- **Siemens SINUMERIK 810D/840D** — PCIN ASCII/punched-tape starting point
  at 19200 baud, 8N1 and XON/XOFF. Archive protocol is not supported.
- **Yasnac i80** — 9600 baud, 7E2 and XON/XOFF.
- **Legacy FANUC 6/10/11/12/15** — conservative 4800 baud, 7E2 and
  XON/XOFF starting point.

These additional library entries are starting points, not universal settings.
Machine builders can change CNC parameters, port wiring, and supported transfer
protocols. Match the profile to the control before connecting or transferring.
The library deliberately excludes proprietary or error-correcting protocols
that C2C does not implement, including Mazatrol conversational transfer,
HEIDENHAIN FE/LSV2, Siemens archive protocol, and XMODEM.

The editor shows an estimated send time before a transfer starts. The estimate
uses the selected profile's filtering, transmitted byte count, serial framing,
baud rate, and line pacing—the same inputs used by the live remaining-time
display.

Loaded programs are monitored for changes made by another editor or CAM system.
When a file changes on disk, C2C DNC asks whether to reload it and warns before
replacing unsaved tab edits. Live transfer statistics use fixed-width,
left-aligned monospaced fields so changing numbers remain visually stable.

C2C DNC accepts NC file paths supplied by Windows file associations and CAM
posting workflows. It runs as a single application instance: posting another
program while C2C DNC is already open sends that file to the existing window,
opens it in a tab, and brings the window forward instead of launching another
copy.

## Build

Use the .NET 10 SDK on Windows x64 and run `powershell -File .\build.ps1`.
This publishes a self-contained Windows x64 folder to `publish`.
Run `C2CDncOpenPreview.exe` from that folder. No .NET installation is needed
on the target PC; USB/RS-232 adapter drivers may still be required.

## Use

Extract the complete application ZIP to a folder before launching
`C2CDncOpenPreview.exe`. The executable must remain beside its `.dll`, `.json`, and
`System.IO.Ports.dll` companion files.

1. Connect a proper USB-to-RS-232 adapter and confirm the CNC cable pinout.
2. Select the CNC profile. Its default COM port appears automatically.
3. If Windows assigned another port, type it into the COM port box.
4. To send the whole program immediately, choose **Send program**. The small
   arrow beside it offers partial sends from the current line or preceding
   tool change; partial sends require an extra safety confirmation.
5. To receive, prepare the CNC to punch/output, choose **Receive**, then start
   output on the control. Save the result with **Save as…**.
6. **Stop** cancels either operation and closes the serial port.

The editor gives G and M commands separate colors, assigns individual colors
to X/Y/Z/A/B/C axes, and also highlights feed values, sequence and program
numbers, other address words, `%` delimiters, and comments. Use **Ctrl+O** to
open one or several programs; every selected file opens in its own tab. Use
**Ctrl+S** to send the active program. **Ctrl+F** opens Find,
**Ctrl+H** opens Find/Replace, **F3** finds the next match, and **Shift+F3**
finds the previous match.

The navigation toolbar jumps forward or backward through tool changes and
Fusion-style operation comments such as `(2D CONTOUR4)` and `(FACE1)`. Quick
navigation starts with **Next operation** and also locates M00/M01 stops, program ends, spindle
commands, and coolant commands. **Top** or **Ctrl+Home** returns immediately to
the beginning of the active program. The NC Outline at the right lists all of
these targets; double-click an entry to jump to its line. Use its **−** and
compact **Width −** and **Width +** controls to resize it, and use the horizontal scrollbar for long labels.
A recent-file list keeps the last 12 programs handy.

Multiple programs can remain loaded in tabs. Use **+** or **Ctrl+N** for a new
tab, the **×** on any tab to close that program, or **Ctrl+W** to close the active
tab. Hover over a tab to see the program's full file path. Drag tab headers to reorder
them, or right-click the tab strip to close a tab or close all. Modified tabs
show `*` and require confirmation before they are discarded. Each tab retains
its cursor, scroll position, Find/Replace settings, and NC Outline. Sending and
receiving always use the clearly selected active tab; incoming programs open in
a separate tab.

The **Dark mode** switch changes the entire application theme and remembers
the choice; on first launch it follows the Windows app theme.

The header identifies **Open Preview** and unlimited transfers. **About**
contains preview notes and the CNC/RS-232 safety disclaimer, with no purchase
or registration page. The original commercial/evaluation app is not changed.
This preview starts with separate preferences and profiles, avoiding changes
to the existing edition's data. Add or configure profiles in this preview as
needed. Existing counters are never consulted.

Syntax coloring is applied only to the visible editor area plus a small scroll
buffer. This keeps editing and scrolling responsive even for very large CNC
programs; additional lines are highlighted automatically as they come into
view. For exceptionally large documents above one million characters, coloring
is automatically bypassed to prevent the editor from locking up; all editing,
navigation, transfer, and outline features remain available.

During a send, the status bar shows an estimated time remaining based on the
baud rate, serial framing, bytes still queued, and the profile's line delay.
XON/XOFF pauses initiated by the CNC can make the actual transfer longer.
Receive operations show elapsed time because the sender does not announce a
total program size.

The upper transfer strip fills from left to right with bright state colors,
while the information band below remains clear and readable. Large separate
readouts report percentage, lines, bytes, current character rate, remaining or
elapsed time, and serial errors. Completed transfers retain their total time.
Current activity and completion messages appear in the full-width footer below
the program editor, where they remain readable independently of the transfer panel.
The band also shows live
RX/TX/RTS/CTS/DTR/DSR indicators. Active serial lights flash during a transfer.
**Debug** opens a raw serial viewer that makes CR, LF, NUL, XON, and XOFF
characters visible. **Log** shows the transfer history saved under
`%LOCALAPPDATA%\C2CDncOpenPreview\transfer.log`.

A large color-coded state panel remains visible as **READY**, **SENDING**,
**RECEIVING**, **COMPLETED**, **STOPPED**, or **FAILED**. **Sounds…** enables or
mutes alerts, chooses from eight completion/failure sounds—including two
original laundry-appliance-style melodies—previews them, and
sets their volume. The chosen settings are remembered for the current Windows
user.

The C2C mark is embedded in the executable, taskbar/title-bar icon, and program
header, with separate high-contrast artwork for light and dark themes.

## Profiles

- Choose **Edit profile** to change any setting on the selected built-in or
  custom profile.
- Choose **Add profile** to copy the selected profile as a starting point for a
  new CNC.
- Choose **Shown profiles…** to decide which library and custom profiles appear
  in the main dropdown. Hidden profiles remain available and can be shown again.
- Custom profiles can be removed with **Delete**. Built-in profiles
  cannot be deleted, but their settings can be edited.
- Profiles are saved for the current Windows user under
  `%LOCALAPPDATA%\C2CDncOpenPreview\profiles.json`. Profiles are kept separate from every other C2C DNC edition.

Preset references: [Haas Mill Operator's Manual](https://www.haascnc.com/content/dam/haascnc/en/service/manual/operator/english---mill-operator%27s-manual---2009.pdf),
[FANUC Series 0i Model F Parameter Manual](https://www.drivesul.com.br/template/imagens/manuais/manuais-fanuc/fanuc-series-0i-model-f/FANUC%20Series%200i%20MODEL%20F%20-%20PARAMETER%20MANUAL%20.pdf),
and [Siemens SINUMERIK 802D sl Instruction Manual](https://support.industry.siemens.com/cs/attachments/104946900/802Dsl_BE_0505_en.pdf).

Only one application can open a COM port at a time. Close terminal programs or
other DNC software before starting an operation.

## Hardware note

RS-232 voltage levels are not TTL/USB serial levels. Use a genuine RS-232
adapter and the cable/pinout required by the CNC. Verify protective ground,
shielding, null-modem/straight-through wiring, and hardware keying before
connecting valuable machine controls.
