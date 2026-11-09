# Tasks: Community configurations

## Phase 1: Setup
- [x] T001 Create feature branch and specify acceptance, preserved behavior and risk in spec.md.
- [x] T002 Review clarification needs, resolve plan templates, and write plan/research/data model/contracts/quickstart.

## Phase 2: Foundation
- [x] T003 Implement complete portable DTO and strict parser in CommunityConfiguration.cs: file maximum 64 KiB; depth 16; unknown/duplicate members rejected; settings bounds from data-model.md; bounded text and required context.
- [x] T004 Add synthetic boundary, round-trip and privacy tests in Tests/CommunityProfiles.

## Phase 3: US1 Share setup
- [x] T005 [US1] Add fresh contribution form, all-context validation, plain-text full preview and explicit save in ShareProfileDialog.cs.
- [x] T006 [US1] Integrate selected-profile sharing entry point and busy gating in MainForm.cs; do not automatically copy name/port/guidance/source.
- [x] T007 [US1] Document manual private submission and review checklist in docs/COMMUNITY-CONFIGURATIONS.md and issue template; no upload/network action.

## Phase 4: US2 Safe import
- [x] T008 [US2] Implement source preview, untrusted status, local name/COM selection and fresh-copy conversion in CommunityProfilesDialog.cs/core.
- [x] T009 [US2] Integrate transactional candidate-list save before in-memory replacement in MainForm.cs, preserving visibility and old profiles; cancellation/failure changes nothing.
- [x] T010 [US2] Verify malformed/version/bounds/duplicate input, repeated import, write failure and default profile round-trips in tests.

## Phase 5: US3 Reviewed catalog
- [x] T011 [US3] Implement strict embedded catalog validation, stable IDs/revisions, distinct statuses/evidence and honest empty catalog in CommunityCatalog.cs and Assets/community-configurations.json.
- [x] T012 [US3] Display catalog/source/evidence/cautions; do not mark local edits currently verified. Test catalog status/identity/evidence failures and empty state.

## Phase 6: Verification and polish
- [x] T013 Integrate checks with CI, documented build instructions and source packaging; no new dependencies.
- [x] T014 Run locked restore, final Release build, community tests, original serial simulation, source hygiene and publish. Record outcomes and exact source hashes in verification.md.
- [ ] T015 Exercise disconnected UI repeated/open/cancel/import/export flows and capture screenshots where supported; disclose unrun stages.
- [x] T016 Review final diff, independent second review and convergence against every requirement; record remaining limitations and rollback.

Dependencies: T001-T002 before implementation. T003 core API precedes UI/tests; T004/T007 may run independently. T005-T012 integrate before T013-T016. Parallel workers own disjoint files. No task authorizes push/merge/public release or hardware operations.

T015 remains partially complete: current light/dark forms, required validation, default-size wrapping/actions, private-field exclusion, Back/edit, simulated share/repeat and Cancel were observed; full GUI matrix remains unrun. See verification.md for exact passed, failed and unrun stages. No implementation gap found in final convergence; no duplicate convergence task appended for this existing open validation item.

## Phase 7: User-requested simplification (2026-11-09)
Prior completed tasks describe the first iteration at 008ae8e; the following tasks reconcile the latest scope before delivery.
- [x] T017 Refine spec.md, plan.md and analysis for one-field GitHub sharing, two labels and unchanged safety boundaries.
- [x] T018 [US1] Add pure quick-contribution and fixed-destination post generation in CommunitySharing.cs: description maximum 120, optional notes maximum 2000, explicit unprovided context, no inferred test claims, no user data in URL, Markdown-safe exact JSON.
- [x] T019 [US1] Simplify ShareProfileDialog.cs and MainForm.cs entry wording; privacy preview, explicit copy then launch, private/access/sign-in warning, failure/re-entry/cancel handling and optional save.
- [x] T020 [US3] Show Confirmed / Not confirmed in CommunityConfiguration.cs, CommunityCatalog.cs and CommunityProfilesDialog.cs; preserve confirmation invariants and no inherited local badge.
- [x] T021 [P] [US1] Update docs/COMMUNITY-CONFIGURATIONS.md, README.md, .github/ISSUE_TEMPLATE/configuration_submission.md and spec design/quickstart/contract for paste-based private submission.
- [x] T022 Add helper/privacy/Markdown/status/action-boundary tests in Tests/CommunityProfiles; run final aggregate build/regression/hygiene/publish checks and record verification.md.
- [x] T023 Independently review final source and reconcile all revised acceptance requirements in specs/002-community-configs/verification.md; record manual GUI gap, no push/publication and rollback.

Dependencies: T017 before T018-T021. T018 API precedes T019 integration; T020 can run alongside UI in disjoint source files. T021 is parallel documentation. T022/T023 follow integrated source. Existing T015 remains the manual GUI matrix; do not duplicate or mark passed without observations.

## Visible review correction
- [x] T024 [US1] Correct ShareProfileDialog.cs review layout after visible QA found unconstrained AutoSize text expanding the single column; preserve readable wrapping and visible primary actions at default/minimum size, rerun synthetic/hidden tests and record bounded visible retest in verification.md. Default-size light/dark retest passed; minimum-size/DPI matrix remains explicitly open under T015.
