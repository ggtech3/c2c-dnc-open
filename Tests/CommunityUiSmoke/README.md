# Disconnected community UI harness

Run on Windows with .NET 10:

    dotnet run --project Tests/CommunityUiSmoke/CommunityUiSmoke.csproj -c Release

The harness opens the actual production ShareProfileDialog and CommunityProfilesDialog with a synthetic in-memory list. Imported copies stay in that list only. It never instantiates MainForm/ProfileRepository/SerialDncService, opens a COM port, or reads/writes the user's profile store. File import/export still use real file dialogs: choose only a synthetic fixture and a temporary output path. It also supports light/dark theme inspection.

Check blank-form validation, cancel/reopen, review/back, save-dialog cancellation, JSON export, empty catalog, invalid file rejection, local COM/name validation, import/cancel, repeated import and duplicate-name rejection. Do not use production setup details or NC files. This is manual GUI smoke coverage, not an automated pass or hardware validation. Core automated checks remain in Tests/CommunityProfiles.
