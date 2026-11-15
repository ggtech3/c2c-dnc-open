# Contributing to C2C DNC

C2C-owned source and starter profiles use the [MIT License](LICENSE). See [LICENSE-STATUS.md](LICENSE-STATUS.md) for third-party scope and publication status.

## Setup and verification

Use Windows x64, Git, PowerShell 7 (pwsh) and the .NET 10 SDK. Clone this repository, then run from its root:

~~~powershell
dotnet restore .\WindowsDnc.csproj --locked-mode
dotnet build .\WindowsDnc.csproj -c Release --no-restore
pwsh -File .\Tests\CommunityProfiles\Run.ps1
pwsh -File .\Tests\SerialSimulation\Run.ps1
pwsh -File .\tools\Check-SourceHygiene.ps1
pwsh -File .\build.ps1
~~~

The published application is publish/C2CDncOpenPreview.exe. Keep that folder together. Generated build/test files are ignored. The application uses a separate C2CDncOpenPreview directory beneath LocalAppData, not the commercial edition's settings.

See [docs/SPEC-KIT.md](docs/SPEC-KIT.md) for the specification workflow once installed. Specifications should describe user needs and acceptance, then plan and test changes before implementation.

## Propose a change

1. Open an issue with a reproducible problem or focused proposal. Use synthetic NC samples and remove customer, machine, account and licensing information.
2. Create a short-lived branch from main. Keep each pull request focused.
3. Add or update a specification for behavior changes and tests for regressions.
4. Run the commands above and include the results, environment, limitations, screenshots for UI changes and a rollback note.
5. Open a draft pull request. A maintainer reviews the final diff and evidence before merging.

Do not change serial behavior or default profiles incidentally. Cancellation, interruptions, partial transfers, timeouts, duplicate requests and integrity deserve explicit review. Never test against a live CNC without separate owner authorization and a controlled validation plan.

## Review expectations

Write clear code, match nearby style, explain non-obvious tradeoffs and preserve unrelated changes. Be respectful and specific in review. Do not post security details in ordinary issues; follow SECURITY.md. Maintainers may defer changes that expand scope or lack evidence.

Do not add code or assets you lack rights to contribute. Submit C2C project contributions under the MIT License only when you have the rights to do so; preserve any third-party attribution and applicable terms. No CLA, DCO or copyright assignment is imposed by this document.

## Configuration contributions

Use **Share**, review the sanitized post, then **Copy and open GitHub**, paste and submit. No file attachment, branch or pull request is required for a profile report. A GitHub account is required to post, but not to download public starter files. See the [review procedure](docs/COMMUNITY-CONFIGURATIONS.md). Do not submit raw runtime profiles, programs or logs. Reports start **Not confirmed**; text claims alone never award **Confirmed**. Starter profiles are unverified; only reviewed contributor and independent real-hardware evidence can qualify a profile for independently confirmed status. The three owner-reported tested setups retain their narrower user-reported label. Publishing a download or passing software tests is not verification.
