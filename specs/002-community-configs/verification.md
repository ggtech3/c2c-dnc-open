# Community profile sharing: verification and handoff

## Current scope and environment
Refined 2026-11-09 on private branch `002-community-configs`, starting from first iteration `008ae8e` and main baseline `826d5a57ccffd12ebaf70a821e78ba8205c31b31`. Version remains `0.9.8-open-preview.1`, JSON schema 1. Windows x64, .NET SDK 10.0.303, PowerShell 7. The final source commit is supplied in the handoff/PR rather than recursively embedded here.

The user requested fewer fields and simpler labels, then authorized private push/pull/merge. Source publication remains subject to normal draft-PR and maintainer review. Public release, access changes and hardware operation are not authorized.

## Intended and preserved behavior
Share has one blank required public machine/controller description and optional notes. Exact-post privacy preview precedes copying and opening a fixed private GitHub issue page. The user signs in if necessary, pastes and submits. No app authentication, automatic post/upload, data-bearing URL or website is added. Optional JSON save remains. Visible status is Confirmed / Not confirmed; proof requirements and lack of inherited local confirmation remain unchanged.

All 18 default definitions, SerialDncService, DncTextProcessor, ProfileRepository, AppSettingsRepository and the application dependency lock remain byte-identical to main baseline. Import remains bounded, data-only, previewed and new-copy-only. No existing settings, hidden profile choices, programs or commercial-edition data were changed. Catalog is intentionally empty.

## Passed checks
Local checks on 2026-11-09, rerun after the visible-QA correction:
- Locked application restore and Release build: pass, zero warnings/errors.
- `Tests/CommunityProfiles/Run.ps1`: **23 groups passed, zero failed**. All original tests plus short-input mapping, honest missing context, all-original round trips, private-field exclusions, fixed destination/no URL payload, exact fenced JSON against Markdown/HTML/mentions/links/Unicode, input bounds, two visible statuses and Windows CRLF normalization preserving exact JSON and reviewed/copied text.
- Injected dispatch tests: exact copy-before-open, no construction side effects, success suppresses repeat/reentry/concurrent launch, clipboard failure prevents opening, browser failure preserves copied outcome, explicit retry and guard cleanup. No real clipboard, browser or GitHub calls.
- `Tests/SerialSimulation/Run.ps1`: **21 passed, zero failed**, fake endpoint only. No engine changes since this run.
- Hidden `Tests/CommunityUiSmoke --check-controls`: light/dark construction, exactly two blank bounded inputs, cancel wiring/no callbacks, empty catalog/disabled Add, no imported result. No Show/ShowDialog or desktop focus. CI includes these checks.
- Source hygiene: pass for 128 source files, including new files. This is limited path/pattern checking, not a comprehensive security audit.
- `git diff --check`: pass. Preserved-file comparison to main baseline: pass.
- Self-contained win-x64 publish: pass to separate `artifacts/Community Simplified Candidate` path with spaces; previous candidates and `publish/` untouched. Rebuild from final commit for a separately identified deliverable.

Commands (process-local SDK telemetry opt-out used for final aggregate run):

```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
dotnet restore WindowsDnc.csproj --locked-mode
dotnet build WindowsDnc.csproj -c Release --no-restore
pwsh -File Tests/CommunityProfiles/Run.ps1
dotnet restore Tests/CommunityUiSmoke/CommunityUiSmoke.csproj --locked-mode
dotnet run --no-restore --project Tests/CommunityUiSmoke/CommunityUiSmoke.csproj -c Release -- --check-controls
pwsh -File Tests/SerialSimulation/Run.ps1
pwsh -File tools/Check-SourceHygiene.ps1 -IncludeUntracked
git diff --check
pwsh -File build.ps1 -OutputDirectory 'artifacts/Community Simplified Candidate'
```

## Review and risk
Independent test/UI source review found no blocking issue. Coordinator reviewed all production differences, template/docs, data boundaries and least-privilege CI; no new package dependencies. User data stays out of URLs; untrusted notes cannot close the generated code fence. Clipboard history/sync and private GitHub access are disclosed. Browser opening is described as requested, never proof of submission. Minimal notes cannot manufacture machine-test results.

Confirmed still requires a separately identified tester and at least one operation both testers reported Passed, with dates and evidence. This validates the structure of evidence, not the truth of a report. Original defaults are not retroactively certified. Local copies have historical provenance and no live confirmation state.

Spec/plan/tasks were refined before changes, with all nine functional requirements mapped. No implementation gap found at convergence. T015 remains the manual GUI matrix, not marked complete by hidden controls or delegate tests. Constitution checks pass without exception.

## Interrupted / unverified checks
An initial aggregate run was blocked at an SDK telemetry endpoint. No dotnet process remained; final checks reran with process-local telemetry disabled. No system-wide telemetry setting was changed.

The earlier full GUI session had partial empty-catalog/cancel observations, then stopped when native targeting was unreliable and user desktop activity appeared. That earlier form has now changed. Do not transfer its visual pass to the new form. The current visible retest passed light/dark default-size forms, blank required validation, wrapped privacy and all actions visible, private-field exclusion, Back/edit refreshed preview, harmless simulated copy/open feedback and disabled repeat, Cancel and unchanged 18-profile synthetic count. Full keyboard flow, minimum-width/DPI matrix, real clipboard contention, application default-browser launch, GitHub sign-in interruption/paste/Submit and actual file-dialog sequence remain untested. The fixed submission URL was separately opened read-only in the signed-in browser and showed a blank issue with the intended title and description field; no issue was created. The manual harness now replaces clipboard/browser callbacks with harmless status updates; file dialogs still require synthetic paths.

Real profile-directory failure and restart persistence, clean-machine installation, signing/SmartScreen, adapter drivers/electrical wiring, hardware flow control and CNC send/receive/drip-feed remain untested. No actual profile was submitted or machine exercised. These checks make no hardware compatibility claim.

Remote CI/PR state is recorded separately after the authorized push. A local pass is not a hosted CI pass. No merge should bypass the maintainer's final review requirement.

## Rollback
No profile-store migration. Leave the branch unmerged or revert its feature commits. Imported custom copies can be removed through existing controls; old profiles were not overwritten. The previous candidate remains available. First-iteration evidence and historical hashes are retained in commit `008ae8e`; current packaging must identify its own exact source commit.

## Visible-QA correction (2026-11-09)
After initial hosted CI passed for fd8fbc4, actual review-window inspection found an AutoSize privacy label could expand the unconstrained single column and push the primary button beyond the visible form. The correction adds a full-width percent column and constrains the label to actual available width. The post is normalized to Windows CRLF once, with the same string used for preview and copying. Independent review and a Unicode/Markdown regression passed. Default-size visible light/dark retest passed; the original fd8fbc4 package is superseded and must not be distributed. Remaining manual matrix items are listed above, not represented as complete by hidden checks.
