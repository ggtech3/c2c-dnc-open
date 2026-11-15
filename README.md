# C2C DNC

## Download for Windows

### [Download the Windows app (ZIP)](https://github.com/ggtech3/c2c-dnc-open/releases/download/v0.9.8-open-preview.2/C2C-DNC-Open-0.9.8-open-preview.2-win-x64.zip)

**Ready-to-run Windows x64 app · 0.9.8-open-preview.2 · No GitHub account needed**

1. Download the ZIP using the link above.
2. Right-click the ZIP and choose **Extract All**. Keep the extracted files together.
3. Open the extracted folder and run **`C2CDncOpenPreview.exe`**.

You do not need Git, Visual Studio or the .NET SDK. This preview is unsigned.
**Do not use the green Code → Download ZIP button or Source code (zip)** to get the app. Those contain source code, not the ready-to-run executable.

[Release notes and checksums](https://github.com/ggtech3/c2c-dnc-open/releases/tag/v0.9.8-open-preview.2)

## Repository status and development

C2C-owned source and starter profiles are licensed under the [MIT License](LICENSE). Read [licensing status](LICENSE-STATUS.md), [contributing](CONTRIBUTING.md), [security](SECURITY.md), [governance](GOVERNANCE.md) and the [Spec Kit workflow](docs/SPEC-KIT.md).

The source history starts fresh from a sanitized preview snapshot. Private licensing tools, prior commercial Git history, customer data, runtime state and packaged binaries are excluded. Automated serial checks use a fake endpoint; see [simulation limits](Tests/SerialSimulation/README.md). A passing build or simulation is not machine acceptance.


A Windows 11 RS-232 sender/receiver with optional CNC profiles.

This separate Open Preview is based on 0.9.8/v28. It opens directly, with no
registration, activation, customer identity, purchase step, or transfer limit.
Existing trial/paid releases and their data are not changed.

## Get started with profiles

No machine is configured or selected on first launch. Use **Choose verified setup (3 available)** to open the built-in, offline verified list. Review a setup and choose your own local name and COM port before adding it. No internet or GitHub account is needed for those three setups.

Fifteen former presets
are now downloadable **unverified starting points**, not bundled defaults, in
[profiles/starters](profiles/starters). Their settings are not newly validated
by this move. Match every setting to your own controller, adapter and cable.

Files and available release ZIPs from the public repository can be downloaded
without a GitHub account. Open a starter JSON file on GitHub and
use **Download raw file**. In the app choose **Community configurations**,
import the file, review the settings, and enter your own local name and COM
port before choosing **Add profile**. No account, Git installation or online
connection is needed to import or use a downloaded file. Downloading or
importing a profile never starts a transfer.

Existing saved profiles and user imports are retained. You can also choose
**Add profile** to configure your own setup. Neither a downloaded file nor an
edited local copy inherits a verification badge. Three setup-specific profiles (EMCO/Siemens 810T, Yasnac MX-3 and Haas Classic)
are available in the catalog based on the owner's report of real-machine testing.
They are **user-reported tested**, not independently confirmed, and are not
automatically installed or selected. Review the exact setup and choose your own
local COM port before adding one. Independently confirmed status still requires
reviewed evidence from a separate tester. See the [profile workflow](docs/COMMUNITY-CONFIGURATIONS.md).

Machine builders can change parameters, port wiring and supported protocols.
C2C does not implement Mazatrol conversational transfer, HEIDENHAIN FE/LSV2,
Siemens archive protocol or XMODEM. Do not use a plain RS-232 starting point
for those protocols.

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
   StarTech.com adapters are the recommended starting point; see the
   [hardware note](#hardware-note) below.
2. Add or import and review a CNC profile, then select it. Its saved COM port appears automatically.
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
- Choose **Add profile** to copy the selected profile, or create a new local
  setup when none exists. Review all fields before using it.
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

## Community configurations

Select one profile and choose **Share**. Enter a public machine/controller description and optional notes, review the exact post, then choose **Copy and open GitHub**. Sign in if needed, paste into the issue description with **Ctrl+V**, and click **Submit new issue**. No file handling or Git installation is needed; an optional **Save JSON copy** remains available. Posting an issue or comment requires a GitHub account. Downloading files from a public repository does not.

Use **Community configurations** to preview a shared JSON file or bundled entry and add a new custom copy with your own COM port. The 15 unverified starter files live in the repository rather than the installed app; imports never replace existing profiles. Status distinguishes owner-reported tested setups from independently confirmed evidence; external files remain **Not confirmed**. Confirmation covers only the original setup and supported tested operations, not every machine or edited local copy. Three user-reported tested profiles are available for explicit review and addition; none are installed or selected automatically. The app never posts or uploads automatically.

See [sharing, importing and review instructions](docs/COMMUNITY-CONFIGURATIONS.md).

## Hardware note

StarTech.com USB-to-RS-232 adapters are recommended as a starting point. The
project owner reports that they have worked with every configuration tested so
far. This recommendation reflects that experience, not a guarantee for every
adapter model, driver, cable or CNC configuration. Confirm the adapter driver,
cable pinout, controller settings and flow control for your own setup.

RS-232 voltage levels are not TTL/USB serial levels. Use a genuine RS-232
adapter and the cable/pinout required by the CNC. Verify protective ground,
shielding, null-modem/straight-through wiring, and hardware keying before
connecting valuable machine controls.
