# Disconnected community UI harness

Run hidden control checks on Windows with .NET 10 (does not show windows or take desktop focus):

    dotnet restore Tests/CommunityUiSmoke/CommunityUiSmoke.csproj --locked-mode
    dotnet run --no-restore --project Tests/CommunityUiSmoke/CommunityUiSmoke.csproj -c Release -- --check-controls

These inspect the production light/dark dialogs without Show/ShowDialog or a message loop: two blank sharing inputs and bounds, cancel wiring/no callbacks on close, unselected offline verified catalog/disabled Add and no imported result. This does not exercise visible modal review or button clicks.

Run the disconnected production header component checks and optional offscreen renders:

    dotnet run --no-restore --project Tests/CommunityUiSmoke/CommunityUiSmoke.csproj -c Release -- --check-header --output artifacts/header-layout

This checks 64 combinations of 100%, 125%, 150% and 200% explicit font/layout scaling, both themes, minimum/default/wide/constrained widths and repeated resizing. It checks that all controls stay in one row, buttons have equal heights, normal widths need no scrolling, and constrained widths can scroll to both ends without vertical clipping. It also checks branding/content overlap and stable header height. PNGs render an unattached control tree using parked native handles without showing a window. This is component layout coverage, not a full MainForm run or real monitor-DPI switching. No settings store, serial service, clipboard or browser is used.

For explicitly authorized manual GUI inspection, omit `--check-controls`. Do not launch this mode while the user is working on the desktop.

The harness opens actual production dialogs with a synthetic in-memory profile list. Clipboard and browser callbacks are replaced with harmless status updates: Copy only and Copy and open GitHub do not touch the real clipboard or launch any browser. Production dialog success text describes simulated effects in this harness. No GitHub submission is possible through these callbacks. Imported copies stay in memory. The manual harness does not use MainForm or the profile store. Automated hidden checks also instantiate the production MainForm with an isolated temporary data directory and serial-port enumeration disabled, checking the first-run action, no machine selection and disabled transfers. No COM port is opened and no real profile store is accessed.

File import/export still use real file dialogs: choose only synthetic fixtures and temporary output paths. Manual checks include blank validation, cancel/reopen, review/back, exact privacy preview, simulated copy/open, save cancellation, JSON export, unselected catalog, invalid file rejection, local COM/name validation, repeated import and duplicate names. Never use production setup details or NC files.

Core automated dispatch tests in Tests/CommunityProfiles cover copy-before-open, exact payload, repeated/reentrant/concurrent clicks, clipboard failure preventing launch, browser failure, retry and guard recovery using delegates only. Neither harness proves real clipboard contention, browser launch, GitHub sign-in/paste/submission or CNC compatibility.
