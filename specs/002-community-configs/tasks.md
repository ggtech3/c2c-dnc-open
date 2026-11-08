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

T015 remains partially complete: initial empty catalog/cancel and fresh fields observed; full GUI matrix interrupted. See verification.md for exact passed, failed and unrun stages. No implementation gap found in final convergence; no duplicate convergence task appended for this existing open validation item.
