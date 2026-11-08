# Quickstart validation
On Windows with .NET 10, run from the repository root:

    dotnet restore WindowsDnc.csproj --locked-mode
    dotnet build WindowsDnc.csproj -c Release --no-restore
    pwsh -File Tests/CommunityProfiles/Run.ps1
    pwsh -File Tests/SerialSimulation/Run.ps1
    pwsh -File tools/Check-SourceHygiene.ps1 -IncludeUntracked
    pwsh -File build.ps1

Use only synthetic data and disconnected UI checks. Open Share setup, cancel, reopen, fill safe sample context, preview and export. Open Community configurations: empty reviewed catalog should be explained. Import the sample, cancel once, then add with a distinct local name/COM. Import again and verify independent copies. Check persistence after restart without touching commercial data. Test malformed/version-mismatched files and cancelled dialogs. Do not start send/receive or use real CNC files. Exact outcomes and any UI verification limits belong in verification.md.

For disconnected manual GUI checks use Tests/CommunityUiSmoke/README.md. The harness opens real production dialogs with synthetic in-memory profiles and no real profile storage or serial service. Do not start an additional desktop automation session while someone is using the computer.
