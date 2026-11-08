# Feature Specification: Community configurations

**Feature Branch**: `002-community-configs`
**Created**: 2026-11-08
**Status**: Ready for implementation
**Input**: Implement community-contributed or verified configurations in the private preview.

## User Scenarios & Testing

### User Story 1 - Share a selected configuration (Priority: P1)
A machinist exports only the selected profile's transfer settings, enters a public configuration title and controller, machine-side, cable/adapter and test details, and reviews the resulting contribution before manually sharing it with the maintainer.
**Independent Test**: Export a synthetic profile, inspect every exported field, and round-trip the transfer settings.
**Acceptance Scenarios**:
1. Given local profiles and programs, exporting one configuration includes only its transfer settings and deliberately entered contribution details, not COM assignment, local profile identity, paths, programs, logs or account information.
2. Given missing required controller/cable/test context, export explains what is missing and creates no contribution file.
3. Cancelling review or the save dialog leaves profiles and existing files unchanged. Export never sends information online.

### User Story 2 - Safely try a shared configuration (Priority: P1)
A machinist opens a contribution, previews all settings, test claims and cautions, enters a local name and COM port, and explicitly adds a new custom profile without overwriting anything.
**Independent Test**: Import a valid synthetic contribution twice and reject malformed examples without changing existing profiles.
**Acceptance Scenarios**:
1. All accepted imports create a fresh custom identity. Existing built-in, customized and hidden profiles retain their settings and visibility.
2. Malformed, unsupported, oversized, incomplete and invalid-value contributions are rejected before any profile change. Contribution files cannot contain scripts, commands or machine operations.
3. Review is read-only until Add. Cancellation and failed persistence leave profiles unchanged. Repeated imports never replace an existing profile.
4. File-provided claims cannot award trusted review or confirmation status. No serial port is opened by browsing, import or export.

### User Story 3 - Distinguish reported and confirmed configurations (Priority: P2)
A machinist can browse the maintainer-curated catalog and see whether a configuration is user-reported or independently confirmed, what controller/cable setup and operations were tested, and the limitations of the evidence.
**Independent Test**: Use synthetic catalog entries in automated tests for each status and confirm the shipped empty-state message makes no unsupported hardware claim.
**Acceptance Scenarios**:
1. User reports and independent confirmation have separate labels. Independent confirmation requires a separate tester and concrete evidence, not merely maintainer review or a passing software test.
2. Catalog entries are data-only reviewed additions with provenance, revision and review notes. They use the same safe preview/add flow.
3. The initial catalog is honestly empty until real contributions are reviewed. The existing 18 defaults remain available and are not relabeled verified.
4. The maintainer has a documented private submission/review process and a repeatable validation check. No public intake, account requirement, repository publication or website change is introduced.

### Edge Cases
Duplicate names and repeated imports; invalid enum/range/type/missing fields; duplicate or unknown fields; null collections; excessive file/text/depth; claimed verification in a file; catalog collisions; unsupported schema; null idle timeout; unsaved export details; failed writes; busy transfer; edited local copies; empty catalog.

## Requirements
### Functional Requirements
- **FR-001**: Export a single selected profile using an explicit allowlist and user-entered sharing context.
- **FR-002**: Preview the complete contribution and privacy warning before file creation; no automatic submission.
- **FR-003**: Validate versioned bounded data-only files before import; fail closed on unexpected fields and invalid settings.
- **FR-004**: Preview every transfer setting and test claim before explicitly adding a fresh custom profile with locally chosen name/port.
- **FR-005**: Preserve all 18 defaults, customizations, hidden selections, serial behavior and edition isolation. Do not silently overwrite.
- **FR-006**: Separate untrusted file reports, maintainer-reviewed user reports and independent confirmation, with evidence and no universal compatibility promise.
- **FR-007**: Ship an offline reviewed catalog and private submission/review instructions, initially with zero unsubstantiated entries.
- **FR-008**: Disable community mutations during transfers, handle cancellation/repeated actions/persistence errors without partial profile changes, and keep local edits from inheriting a current verification promise.
- **FR-009**: Add automated synthetic boundary, privacy, persistence and regression checks; document hardware and clean-machine limits.

### Key Entities
- Contribution: versioned transfer settings, public title, controller, machine configuration, cable/adapter, test results and limitations.
- Catalog entry: contribution plus maintainer identity, revision, review date and evidence-backed review status.
- Local imported profile: independent editable copy with local identity and COM assignment, retaining a provenance note but no ongoing certification.

## Success Criteria
### Measurable Outcomes
- **SC-001**: Every transfer setting survives an export/import round trip, with zero unrelated local fields in the contribution.
- **SC-002**: Every rejected or cancelled import leaves the original profile list unchanged; two accepted imports yield two new identities.
- **SC-003**: Every displayed trusted status has matching catalog evidence; zero file claims or simulated tests create trusted confirmation.
- **SC-004**: Existing serial simulations and all 18 baseline profile comparisons pass unchanged.

## Assumptions
- First version is offline/manual sharing, not an online submission service. A future optional catalog update must be separately designed and cannot overwrite local profiles.
- Maintainer uses an already-established private channel while licensing and public release remain undecided.
- No new hardware validation is available. No catalog entry or claim will be fabricated.
- Review status describes the original contributed setup, not changes to local copies or a guarantee for other machines.

## Risk and preserved behavior
Untrusted configuration data affects future machine-facing settings. Strict validation, full preview, local COM selection, fresh-copy-only import and explicit cautions reduce accidental use. Free-text details may contain private information, so they are deliberate inputs requiring user review. No new networking or executable payload is accepted. SerialDncService, DncTextProcessor, CncProfile defaults, existing editions and live-machine behavior remain untouched.

## Clarification review
No essential ambiguity requires user input: the already-agreed offline contribution/review model supplies a conservative first release. Online catalog updates, public launch and live-machine testing are deferred.
