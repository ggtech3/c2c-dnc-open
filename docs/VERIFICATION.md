# Initial private snapshot verification

Date: 2026-11-08. Application version: 0.9.8-open-preview.1. Status: verified for the build and simulated-source scope below; private owner review, not a public release.

## Scope and provenance

This repository begins a new history from an explicit allowlist of the separate Open Preview candidate based on the 0.9.8/v28 application. No prior commercial Git history is imported. Application C# sources and four image/icon assets matched the reviewed preview candidate byte-for-byte before Git line-ending normalization. All 18 built-in defaults and the serial engine are unchanged.

Intentional repository adaptations: exclude Tests/**/*.cs from app compilation; declare win-x64 runtime so locked restores are consistent; add dependency lockfiles and locked restores; add contributor docs, CI, hygiene checks, synthetic simulator and official Spec Kit workflow. No working installation, commercial edition, user settings or machine was changed by repository preparation.

## Completed local checks

Environment: Windows x64, .NET SDK 10.0.303, PowerShell 7.

- dotnet restore WindowsDnc.csproj --locked-mode: passed after declaring the runtime identifier.
- dotnet build WindowsDnc.csproj -c Release --no-restore: passed, zero warnings and errors.
- pwsh -File Tests/SerialSimulation/Run.ps1: passed, 21 scenarios, zero failures; includes 20 repeated sends and all 18 profile configurations.
- pwsh -File build.ps1: passed; self-contained win-x64 candidate produced using locked dependencies.
- Source hygiene: candidate files passed path and credential-pattern checks. This is a limited scan, not a complete security audit.
- Second review: app-source/asset equivalence, fresh-history boundary, contributor/license/safety language, PowerShell/JSON syntax and CI configuration reviewed. Minor documentation links/prerequisites were corrected.
- Spec Kit setup and syntax/prerequisite checks: see spec-kit/VERIFICATION.md.

## What this does not establish

No physical serial ports, USB drivers or CNC machines were exercised. Simulation does not establish hardware flow control, timing, electrical correctness or successful machine delivery. No clean-machine installation was tested here. Packaged UI evidence from candidate preparation is separate and not claimed by these source checks. This source checkout is not itself a shareable application package.

Local tooling checks do not establish that hosted GitHub Actions completed; check the run for the exact pushed commit. The Codex CLI was not available for an end-to-end Spec Kit agent dispatch test. Public release, external contributor terms and application licensing remain owner decisions.

## Rollback and evidence

This is an isolated new source repository. Existing editions and their data remain untouched. Revert a repository change or rebuild a prior reviewed commit rather than modifying a known-good installation. Generated test output and source hashes remain local ignored evidence. Do not commit runtime logs or real NC programs.

Confirm the published branch and exact SHA with Git/GitHub when publishing. Repository access settings and checks must be verified separately; documentation is not a branch-protection policy.

## Hosted verification

Private repository creation and main upload were verified on GitHub. Initial remote commit 6401b6bbdbb7268fd6ed8b02e745cd4732be6b42 matched the local root commit. The [initial hosted run](https://github.com/ggtech3/c2c-dnc-open/actions/runs/42533326665) passed build, source hygiene, serial simulation and self-contained publication checks.

That run reported a deprecated Node 20 runtime in checkout v4. The workflow was then updated to official checkout v7.0.1, pinned to 3d3c42e5aac5ba805825da76410c181273ba90b1, which uses Node 24 and retains persist-credentials=false. Check the [Actions page](https://github.com/ggtech3/c2c-dnc-open/actions) for the exact current commit's result. No deployment, public visibility, collaborator grants or new credentials were created.
