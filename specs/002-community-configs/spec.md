# Feature Specification: Simple community profile sharing

**Branch**: `002-community-configs` | **Created**: 2026-11-08 | **Refined**: 2026-11-09
**Status**: Implemented; visible manual GUI validation remains open
**Input**: Preserve original profiles; use Confirmed / Not confirmed; minimize effort for nontechnical machinists; prefer GitHub, not the website.

## User stories and acceptance
### US1 - Share one profile (P1)
Select a profile, choose Share, enter one public machine/controller description and optional notes, review the exact sanitized submission, then choose Copy and open GitHub. Sign in if needed, paste into the issue body and click Submit. No file handling, Git installation, tokens or pull request is required.
- One required description (maximum 120 characters), optional notes (maximum 2000); both initially blank. No private profile name/notes are prefilled.
- Application version is automatic; details not entered are explicitly unprovided, results NotTested and date empty. Prose cannot infer test results or confirmation.
- Preview identifies private ggtech3/c2c-dnc-open as destination and discloses GitHub sign-in/repository access requirements, manual posting, and local clipboard/history exposure.
- Explicit Copy and open GitHub copies the exact reviewed post, then opens a fixed HTTPS issue URL containing no user data. User pastes/submits. No automatic post/upload. Secondary local JSON save remains available.
- Clipboard failure prevents browser launch. Browser failure clearly says text was copied and supports retry. Cancel/Back has no sharing effect. Repeated clicks cannot queue duplicate launches.
- Only selected portable settings and deliberately entered text are shared. COM assignment, local identity/name, guidance/source URL, programs, logs, paths and other profiles are excluded. Entered notes remain inert serialized JSON text, not active Markdown.
Independent test: minimal synthetic submission round-trips settings, exclusions, fixed URL, cancellation and action failures without real clipboard/browser use.

### US2 - Safely try a shared profile (P1)
Keep strict version-1 JSON validation, complete preview, explicit local name/COM and fresh-copy import. Preserve all 18 originals, customizations and hidden selections. Cancel or failed persistence changes nothing. No scripts/network fetch/serial operations. External files are Not confirmed regardless of text claims; editable local copies inherit no ongoing confirmation badge.
Independent test: repeated valid import, malformed inputs, failed save and preservation checks.

### US3 - Understand two labels (P2)
Only Confirmed / Not confirmed are visible confirmation statuses. Confirmed requires reviewed bundled evidence from a separate actual tester for the exact original setup and operations both testers reported Passed. Maintainer data review and software tests do not qualify. Failed/untested operations remain outside confirmation. Internal schema fields can remain richer for compatibility. Empty catalog is honest; originals are not retroactively marked confirmed. No automatic updates or replacements.
Independent test: both catalog states, file preview, missing/invalid evidence and local provenance.

## Functional requirements
- FR-001: One-profile sharing with one required public description and optional notes.
- FR-002: Exact-post privacy review with destination, sign-in/access, manual submission and clipboard disclosure.
- FR-003: Bounded strict parser and backward-compatible version-1 packets.
- FR-004: Safe preview/new-copy import with explicit local name/port.
- FR-005: Preserve original profiles, customizations, hidden selections, serial behavior and edition isolation.
- FR-006: Two visible confirmation statuses, evidence-bound original setup only, no ongoing local-copy guarantee.
- FR-007: Fixed GitHub issue destination after explicit action; no automatic posting, authentication service, website or publication.
- FR-008: Handle busy transfer, cancel/back/repeat, clipboard/browser and persistence failures safely.
- FR-009: Synthetic privacy/schema/status/action tests plus baseline regressions; honestly record GUI, hosted and hardware limits.

## Success criteria
- SC-001: All 18 portable settings round-trip with zero unrelated local fields.
- SC-002: Primary sharing needs one required input and no local-file handling.
- SC-003: Zero data-bearing URLs, automatic posts or untrusted confirmation promotions.
- SC-004: Rejected/cancelled imports preserve profiles and original serial simulations/defaults pass unchanged.

## Clarification review
Latest user direction supersedes the long-form offline sharing UI, not its safety boundaries. This feature implements user-triggered browser opening, not automatic submission. Separate owner authorization governs source push, pull requests and merge; no access change or public publication is implied. Existing authorized collaborators alone can use the private repo; public intake is a later owner decision. No essential clarification remains. Import is still offline; only the explicit sharing action opens a browser.

## Risk and preserved behavior
Untrusted settings affect future machine-facing behavior: retain strict parsing, preview, local port selection and fresh-copy persistence. Clipboard/browser are explicit side effects. Keep user text out of URLs, copy only after privacy review, and never equate launch with submission. Use a fixed recipient. Minimal inputs cannot fabricate evidence. SerialDncService, DncTextProcessor, CncProfile defaults, existing profile storage, editions and user data remain unchanged.
