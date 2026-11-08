# Implementation Plan: Private Open Preview baseline

**Feature**: `001-open-preview` | **Date**: 2026-11-08 | [Specification](spec.md)

## Acceptance and risk

Deliver a private sanitized source baseline with unchanged transfer semantics and isolated preview identity. The application can operate physical CNC serial ports, so an apparently small framing or pacing change has material consequences. This work authorizes development and private repository preparation only, not hardware use or public release. Repository/tooling changes must not alter installed commercial releases or runtime data.

## Technical context

- C# WinForms, .NET 10, Windows x64; local filesystem and RS-232 through the existing engine.
- Existing application structure is retained rather than refactored for this baseline.
- Private preview storage: `%LOCALAPPDATA%\C2CDncOpenPreview`.
- Development workflow: GitHub Spec Kit 1.3.2, Codex repository skills, PowerShell scripts; Python tooling is development-only.
- No new network service, protocol, persistent credential or production data fixture.

## Constitution check

1. Preservation: compare serial engine and every field of all 18 profile defaults.
2. Isolation: verify preview identifiers and storage paths; prohibit commercial-data migration.
3. Authorization: keep private visibility; leave license and hardware decisions to owner.
4. Evidence: run final build/regressions and record passed/failed/skipped/untested separately.
5. Hygiene: scan sanitized source and staged history, preserve attribution, second diff review.

## Implementation sequence

1. Establish a clean source snapshot, preview identity and sanitized documentation; exclude original Git history, binaries, private keys and runtime state.
2. Make minimal preview gate/branding changes in the owning application workstream. Preserve serial and profile code; compare with the approved baseline.
3. Add contributor instructions, least-privilege validation CI and explicit license/publication boundaries.
4. Initialize official pinned Spec Kit core with Codex skills and PowerShell. Disable optional native event handlers; install no optional service/Git extension.
5. Review this constitution/spec/plan/tasks against actual evidence, run final applicable checks, and publish only to the authorized private destination.

## Verification strategy

Build and existing regressions first. Use synthetic/disconnected tests for activation removal, edition isolation, profile defaults and serial boundary behavior. Manually inspect final source/config/generated artifacts and perform a second deliberate pass. Existing historical hardware observations are not new preview validation. The parent workstream records exact commands, source revision and application results; this scaffold does not certify those checks.

## Rollback

Keep the commercial installation and its data untouched. Revert a reviewed source change or discard the separate preview candidate. Do not delete or migrate user settings as rollback. Spec Kit can be removed from a future branch by reverting its scaffold/docs and recreating or removing the ignored local virtual environment independently.

## Deliverables

- App source, repository standards and CI: owned by application/repository workstreams.
- `.specify/memory/constitution.md`, upstream templates/scripts/workflow, `.agents/skills/speckit-*`: Spec Kit integration.
- `specs/001-open-preview/`: initial acceptance and planning artifacts.
- `docs/spec-kit/`: pinned setup, provenance, command use and verification limitations.
- `third_party/spec-kit/LICENSE`: upstream license only, not an application license.
