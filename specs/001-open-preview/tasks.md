# Tasks: Private Open Preview baseline

Completed checks link to [repository verification](../../docs/VERIFICATION.md), [preview evidence](../../docs/PREVIEW-VALIDATION.md) and [Spec Kit verification](../../docs/spec-kit/VERIFICATION.md). Unchecked tasks remain unverified or require future owner decisions. Existing implementation can satisfy a task after review; this is not an instruction to redo completed work or change serial behavior. Dependencies: Phase 1 before application verification; final privacy/review gates before private push.

## Phase 1: Baseline and scope

- [x] T001 Record approved source revision, preservation scope and risk in repository change evidence.
- [x] T002 Confirm the clean source tree excludes old history, issuer tools, keys, customer files, runtime state and build outputs.
- [x] T003 Review spec.md, plan.md and the constitution against the owner-approved preview scope.

## Phase 2: US1 unrestricted isolated preview

- [x] T004 Verify preview project/executable identity and isolated AppData paths in project and settings code.
- [x] T005 Verify activation, registration, purchases and transfer-limit gates are absent from preview startup and transfer entry points.
- [x] T006 Run synthetic disconnected startup and edition-isolation regression tests; record untouched commercial data boundary.
- [ ] T007 Review unsaved changes, repeated file opening and single-instance routing using synthetic files; label manual coverage accurately.

## Phase 3: US2 preserve transfer behavior

- [x] T008 Compare serial engine against the approved baseline and explain every meaningful difference.
- [x] T009 Compare all fields of all 18 default profiles against the approved baseline, not only names/count.
- [x] T010 Run available synthetic/disconnected tests for framing, filtering, cancellation, timeouts, partial sends, disconnects, duplicates and integrity; record coverage gaps.
- [x] T011 State explicitly that no physical CNC validation was performed for this preview work.

## Phase 4: US3 contributor-ready private repository

- [x] T012 Verify relative-path build/test instructions, review policy, security contact route and no-license/publication boundary.
- [x] T013 Verify pinned Spec Kit CLI, PowerShell scaffolding, Codex skills and upstream license/provenance using docs/SPEC-KIT.md.
- [x] T014 Review CI permissions, immutable dependencies where practical, no deployment and no secrets on untrusted contributions.
- [x] T015 Run final application build/regressions and record command, environment, outcome and gaps.
- [x] T016 Review final source/config/tooling diff and conduct a deliberate second review; close critical acceptance failures.
- [x] T017 Scan final staged files and clean history for sensitive data and private workstation references before private push.
- [ ] T018 Verify authorized private remote visibility and pushed commit; record remaining validation and owner decisions without making public.

## Future gates (not authorized by this baseline)

- [ ] T019 Obtain owner license choice and explicit approval before public visibility or public distribution.
- [ ] T020 Obtain separate live-machine authorization and controller-specific safety procedure before any physical transfer test.
