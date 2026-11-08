# Community configurations: verification and handoff

## Scope and source
- Date: 2026-11-08 (aggregate checks completed 23:51 UTC; build/community checks/publish repeated after whitespace cleanup at 23:55 UTC).
- Source: private ggtech3/c2c-dnc-open, branch `002-community-configs`, based on `826d5a57ccffd12ebaf70a821e78ba8205c31b31`.
- Version: `0.9.8-open-preview.1`; schema version 1. This is a local review candidate, not a published release.
- Environment: Windows x64, .NET SDK 10.0.303, PowerShell 7. Existing user/commercial profile storage and hardware were not used by the tests.

## Intended and preserved behavior
The feature provides deliberate sanitized single-setup export, complete readable/exact preview, new-copy-only import, an offline curated catalog and a private review procedure. The catalog starts empty. Existing 18 defaults, serial engine, text processor, profile repository and app settings repository are unchanged. No registration, networking, automatic submission, catalog download, live port access or website changes were added.

Risk: imported settings can affect a later machine transfer. Validation and evidence labels reduce malformed-data and misleading-status risks; they cannot prove a human test claim or hardware compatibility. An independently confirmed label requires the same operation Passed by the contributor and distinct confirming tester. Failed/untested operations remain visible and unconfirmed.

## Passed
Final commands against the implementation after trust/preview refinements:

```powershell
dotnet restore WindowsDnc.csproj --locked-mode
dotnet build WindowsDnc.csproj -c Release --no-restore
pwsh -File Tests/CommunityProfiles/Run.ps1
pwsh -File Tests/SerialSimulation/Run.ps1
pwsh -File tools/Check-SourceHygiene.ps1 -IncludeUntracked
git diff --check
pwsh -File build.ps1 -OutputDirectory 'artifacts/Community Candidate'
dotnet build Tests/CommunityUiSmoke/CommunityUiSmoke.csproj -c Release --no-restore -o 'artifacts/Community UI smoke'
```

- Application Release build: 0 warnings, 0 errors.
- Community checks: **14 groups passed, 0 failed**. All 18 defaults and all 18 portable settings round-trip. Tests cover privacy allowlisting, malformed/missing/null/unknown/duplicate members, type/enum/range/version/depth/text bounds, UTF-8 and 64 KiB boundaries, fresh repeated identities, collision rejection, injected persistence errors, catalog validation, independent-operation evidence and single-line spoofing prevention.
- Existing serial simulation: **21 passed, 0 failed**. Fake endpoint only, including cancellation, disconnect, timeout, filtering, framing and preserved baseline behaviors.
- Source hygiene: passed for 124 then-current source files, including untracked feature files. This is limited pattern/path checking, not a complete security audit. Repeated after final evidence: 125 files passed.
- Whitespace diff check: passed. Staging initially found one extra blank line at the end of ShareProfileDialog.cs; it was removed, the staged check passed, and build/community tests/publish were repeated successfully.
- Self-contained win-x64 publish: succeeded to the separate path-with-spaces candidate directory. The existing `publish/` folder was not replaced.
- Disconnected UI harness: built with 0 warnings/errors in its final separate output folder. It uses actual dialogs and a synthetic in-memory list; no real profile repository, serial service or COM port is constructed. Its initial and locked restores succeeded; lock file is included.
- Preservation check: `git diff --exit-code HEAD -- CncProfile.cs SerialDncService.cs DncTextProcessor.cs ProfileRepository.cs AppSettingsRepository.cs packages.lock.json` passed against the baseline before the feature commit.

## GUI observations and limits
A native GUI session opened the synthetic harness and the actual community dialog. The initial empty catalog/cautions were displayed, Add was disabled, and Cancel returned with 18 synthetic profiles unchanged. The Share setup form exposed fresh blank public fields, Not tested defaults and only the application version prefilled. No local title/COM/guidance appeared in its form fields.

The initial preview required horizontal scrolling. Both preview boxes were changed to wrap text with vertical-only scrolling, and the final code rebuilt successfully. The final wrap change was not visually retested.

Further modal interactions were blocked by unreliable native accessibility targeting (child element unavailable in cached state) and the session was stopped when desktop activity was detected. Do not count validation-error dialogs, full export/save/cancel/back, actual file import/add, repeated GUI import, dark-theme layout, main header clipping at minimum width, multi-DPI layout, restart persistence or the final wrapping change as GUI-passed. The included `Tests/CommunityUiSmoke` harness supports completing these checks without changing real profiles. The harness uses ShowInTaskbar=true only for its test dialogs so they can be selected individually by accessibility tools; production dialogs remain modal with their original taskbar behavior.

A first attempt to rebuild the UI harness in its running output folder failed with MSB3027/MSB3021 because its referenced DLL was in use. Building into the separate final output folder succeeded. This was a test-output file lock, not an application compilation failure. No locked user file was deleted or replaced.

## Review and convergence
- Reviewed final production changes and both test projects, docs, issue template, embedded empty catalog and least-privilege CI addition.
- Separate review of UI code found cancellation, busy gating, fresh public inputs, candidate-save-before-list-mutation and offline-only behavior consistent with the spec.
- Additional trust review found a failed-only independent result could previously receive a confirmation label. Fixed by requiring a corresponding Passed operation from both testers; regression tests now cover all failed/untested combinations and mismatched passes.
- Final data contract uses strict unknown/duplicate rejection, no executable fields, literal data display and no file-status promotion. Date/evidence presence does not prove the underlying report; human maintainer review is still required.
- No production package dependencies added. Existing System.IO.Ports version and application lock remain unchanged; test projects reuse that dependency.
- No implementation gaps found against FR-001 through FR-009. The remaining manual GUI verification in T015 is explicitly open; it is not represented as passed.
- Constitution gates rechecked: all preservation/privacy/private-release boundaries retained, no exceptions requested.

## Unrun / unverified
- Remote CI: not run for this branch because no push or PR was authorized.
- Clean-machine installation, distributed package/notices/signing, SmartScreen and public download: not tested or released.
- Actual profile-directory permission/disk-full failure and restart persistence: not exercised. Injected save failure and code review cover the transaction boundary without risking user data.
- Physical adapter/driver, electrical wiring, hardware flow control, CNC send/receive/drip-feed: not tested and not authorized. No hardware confirmation entry was added.
- Full manual GUI matrix described above remains for owner review or a later uninterrupted disconnected test.

## SHA-256 evidence (pre-commit candidate)
- CncProfile.cs: `980864753EBCE24CF994A84508568A83B57F10580393969555B92E133D301A00`
- SerialDncService.cs: `0D221F48DC604D741AEB14BEF0992FB6A77DFD9098F6EBADCB14A081170262C2`
- DncTextProcessor.cs: `B09A647B1116F6D192E88B4C931ECD856AE56C01086460444DA536C6DDAF08E0`
- CommunityConfiguration.cs: `BF71904D3CC8DB67663B2B01DD3F53AFE6D5DCAB2CA981F62CA11CBA35CE1699`
- CommunityCatalog.cs: `9ECC3677F8403B2BA01674E1B7933135D841962E958072A97EBEB801644A442E`
- CommunityProfilesDialog.cs: `68E69F9C29716AC41F7D736802BB40743AA5606C3277D99BD8A4B41CFF266860`
- ShareProfileDialog.cs: `EA3139768C54734CB3CC4B5D9E1D96808571028B36E3B37451DE7E0901E45F15`
- MainForm.cs: `798543EAC10E3915B6F136CF92BB87ACECA092506C5D14333D2D616B1B4B361E`
- Candidate exe: `9B532C2D9EBEE18F566BA48446533ABF2B33D6A00406D0139B84A053892F6128`
- Candidate application DLL: `CBD7A9819EBFB2F9EECADD2A836AA7937F3D953F5AF4FC3C4E6A207744207D55`

Candidate was built before the feature commit, so embedded Git informational metadata identifies the baseline. The hashes above identify the actual tested candidate. Rebuild from the eventual review commit for any distribution.

## Rollback and publication state
This branch can be left unmerged or its feature commit reverted. There is no profile-store migration. Any deliberately imported profile is an ordinary removable custom copy; old profiles were never overwritten by import. Keep the prior application candidate and user data. No source was pushed, no PR created, no merge or deployment performed, no repository access or visibility changed, and no public license adopted. Final commit ID is supplied in the handoff rather than embedded recursively in this document.
