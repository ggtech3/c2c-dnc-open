# Disconnected community UI harness

Run hidden control checks on Windows with .NET 10 (does not show windows or take desktop focus):

    dotnet restore Tests/CommunityUiSmoke/CommunityUiSmoke.csproj --locked-mode
    dotnet run --no-restore --project Tests/CommunityUiSmoke/CommunityUiSmoke.csproj -c Release -- --check-controls

These inspect the production light/dark dialogs without Show/ShowDialog or a message loop: two blank sharing inputs and bounds, cancel wiring/no callbacks on close, empty catalog/disabled Add and no imported result. This does not exercise visible modal review or button clicks.

For explicitly authorized manual GUI inspection, omit `--check-controls`. Do not launch this mode while the user is working on the desktop.

The harness opens actual production dialogs with a synthetic in-memory profile list. Clipboard and browser callbacks are replaced with harmless status updates: Copy only and Copy and open GitHub do not touch the real clipboard or launch any browser. Production dialog success text describes simulated effects in this harness. No GitHub submission is possible through these callbacks. Imported copies stay in memory. It never instantiates MainForm/ProfileRepository/SerialDncService, opens a COM port, or reads/writes the user's profile store.

File import/export still use real file dialogs: choose only synthetic fixtures and temporary output paths. Manual checks include blank validation, cancel/reopen, review/back, exact privacy preview, simulated copy/open, save cancellation, JSON export, empty catalog, invalid file rejection, local COM/name validation, repeated import and duplicate names. Never use production setup details or NC files.

Core automated dispatch tests in Tests/CommunityProfiles cover copy-before-open, exact payload, repeated/reentrant/concurrent clicks, clipboard failure preventing launch, browser failure, retry and guard recovery using delegates only. Neither harness proves real clipboard contention, browser launch, GitHub sign-in/paste/submission or CNC compatibility.
