# Quickstart validation
On Windows with .NET 10, run from the repository root:

    dotnet restore WindowsDnc.csproj --locked-mode
    dotnet build WindowsDnc.csproj -c Release --no-restore
    pwsh -File Tests/CommunityProfiles/Run.ps1
    pwsh -File Tests/SerialSimulation/Run.ps1
    pwsh -File tools/Check-SourceHygiene.ps1 -IncludeUntracked
    pwsh -File build.ps1 -OutputDirectory 'artifacts/Community Simplified Candidate'

Use synthetic data and disconnected checks. Share starts with one blank public machine/controller description and optional notes. Review exact text, cancel/back, revise it and verify fresh preview. Check keyboard navigation, minimum size/DPI/dark theme, action failure/retry and duplicate clicks. Do not invoke a real clipboard/browser action during unattended tests. Do not actually post to GitHub. Sign-in/access/paste/Submit behavior is a separate manual check with explicit operator participation.

Community configurations explains the empty catalog; imported files show Not confirmed. Open a synthetic file, cancel once, then add with distinct local name/COM. Reimport another copy. Check restart persistence only in an isolated test data directory. Malformed/version-mismatched files and cancellation must preserve existing profiles. No send/receive or real CNC files.

See Tests/CommunityUiSmoke/README.md for the synthetic harness and its current action boundaries. Do not take over the desktop while the user is active. Record exact passed/failed/untested outcomes in verification.md.
