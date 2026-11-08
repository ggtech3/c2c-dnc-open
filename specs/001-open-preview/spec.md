# Feature Specification: Private Open Preview baseline

**Feature**: `001-open-preview`
**Created**: 2026-11-08
**Status**: Acceptance baseline; verification tracked separately
**Scope**: A sanitized, contributor-ready private source repository for the unrestricted local preview. Public release and application license selection remain owner decisions.

## User scenarios and acceptance

### US1: Use the preview without activation (P1)
As an authorized reviewer, I can launch the Windows application offline, open a synthetic NC file, and use its editor without registration, purchase prompts, customer identity, or transfer quotas.

- Given a clean preview data directory, launch opens the main window directly.
- Given existing commercial-edition data, preview startup does not read, migrate, overwrite, or delete it.
- No preview workflow consults activation files, license counters, or an online service.

### US2: Preserve transfer behavior (P1)
As an operator reviewing the preview, I retain the established serial engine and all 18 built-in profile defaults.

- Compare all default profile fields, not just names or profile count, against the approved source baseline.
- Preserve baud, framing, parity, stop bits, flow control, DTR/RTS, delimiters, filtering, pacing, receive completion and parity handling.
- Preserve partial-send confirmation, cancellation, timeout, disconnect, duplicate-prevention and integrity behavior.
- Use synthetic fixtures and disconnected or simulator checks first. No acceptance claim here implies a physical controller test or authorization to transfer to a CNC.

### US3: Review and contribute safely (P2)
As an authorized contributor, I can understand, build and review the source without private licensing tools, secrets, customer data, absolute workstation paths or development history.

- Document Windows/.NET prerequisites, build/test commands and remaining verification gaps.
- Include contribution, safety, security-reporting and review guidance plus this spec-driven workflow.
- Preserve third-party notices separately from application licensing. The repository remains private until the owner approves visibility and a license.

## Requirements

- FR-001: Run as an offline local Windows WinForms application targeting .NET 10; no new network runtime dependency.
- FR-002: Use the executable identity `C2CDncOpenPreview` and isolated `%LOCALAPPDATA%\C2CDncOpenPreview` runtime storage.
- FR-003: Remove preview activation/registration/purchase/transfer-limit gates without changing serial or profile semantics.
- FR-004: Preserve all 18 default profiles field-for-field and record the comparison method.
- FR-005: Preserve editor, file-open/single-instance routing, unsaved-change handling, receive and send UI behavior except explicitly approved preview branding/gates.
- FR-006: Keep source, development tools, distributables and private runtime data separate. Exclude keys, issuer tools, customer programs and licensing/customer records.
- FR-007: Use a clean repository history, least-privilege CI and reviewable changes. Do not represent private publication as a test pass.
- FR-008: Track acceptance, risk, evidence, exceptions and rollback for each change. Review final diffs twice.
- FR-009: Spec Kit is development-only. No Python or Spec Kit dependency is required by the shipped DNC app.

## Edge cases and safety boundaries

Check repeated launch/file-open requests, empty/malformed/large synthetic files, unsaved edits, cancellation, unavailable/busy COM ports, interrupted transfers, receive idle completion, duplicate send initiation and independent edition settings. Hardware checks require separate explicit approval and a controller-specific procedure.

## Success criteria

- SC-001: Review finds no active preview activation/registration/quota path; disconnected launch reaches the main UI.
- SC-002: Serial-engine comparison and 18-profile field comparison pass or every difference is explicitly reviewed and approved.
- SC-003: Build and available automated regression tests pass on the final source; unrun manual/hardware checks remain visibly untested.
- SC-004: Source and staged-history privacy scans and a second review complete before private publication.
- SC-005: Another authorized contributor can follow relative-path instructions and invoke the installed Spec Kit skills.

## Non-goals

No new CNC protocol, serial refactor, automatic settings migration, public distribution, license grant, live-machine test, CI deployment or signing-key work.
