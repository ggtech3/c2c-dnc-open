# Header button layout correction

## Acceptance and scope

- Owner update: all five header controls occupy one non-wrapping action row.
- The single row fits at the minimum/default window width and 100%, 125%, 150%, and 200% layout scaling. If unusually large text or a narrower viewport exhausts the available width, a horizontal scrollbar keeps every action reachable without wrapping, clipping vertically or covering the branding/profile card.
- Light and dark themes retain the existing colors, labels, action handlers and tab order. Repeated resizing must not grow the header cumulatively.
- Serial behavior, all 18 built-in profiles, text processing, saved settings and existing releases remain unchanged. No machine commands or live COM connections are part of validation.

## Risk and approach

The original header combined an absolute-height root row with a wrapping action panel. A first correction measured two action rows, but the owner now requests one row. Replace the two groups with one non-wrapping, horizontally scrollable action strip beside the branding. Reserve enough height for both controls and the scrollbar. Risk is limited to header sizing and available action width; exercise normal and deliberately constrained widths, both themes, scrolling and repeated resizing in a disconnected component harness.

One-row component verification is recorded below. The older two-row preview remains in its separate folder for rollback.

## Validation

Source baseline: main at 392d436 (includes f860c28), version 0.9.8-open-preview.1. Windows x64, .NET SDK 10.0.303, 2026-11-12. Work stays local on fix/header-button-clipping until separately authorized for publication.

- PASS: locked app restore and Release build, zero warnings/errors.
- PASS: `Tests/CommunityProfiles/Run.ps1`, 23 synthetic checks including all 18 defaults.
- PASS: `Tests/SerialSimulation/Run.ps1`, 21 fake-endpoint scenarios.
- PASS: CommunityUiSmoke `--check-controls`, hidden light/dark dialog checks.
- PASS: CommunityUiSmoke `--check-header`, 64 component layout cases; minimum/default/wide/constrained widths, explicit 100/125/150/200% font/layout scaling, light/dark and repeated resizing. All actions stay in one row with no scrollbar at normal widths. Constrained cases verified horizontal scrolling to both ends and back without vertical clipping. Offscreen one-row renders inspected at 100% light and 150% dark.
- PASS: source hygiene including untracked source, and `git diff --check`.
- PASS: self-contained Windows x64 publish to the separate ignored `artifacts/C2C DNC Single Row Preview` directory, a path containing spaces. Executable and required companion files verified. Existing preview folders and runtime data were not replaced.
- Final diff/second review: header containers and sizing only; existing labels, event handlers, button groups and action order retained. No serial, profile, text-processing, dependency or licensing changes. No credentials, runtime data or binaries added to source.

Limitations: explicit scaling tests are not physical monitor-DPI switching. No full-app GUI launch, clean-machine launch, real COM port, USB driver, flow control or CNC validation. The header component tests do not claim coverage of unrelated dialogs.

Rollback: continue using the prior preview folder; no settings migration or replacement occurred. The source change is local and uncommitted, with no push, PR, merge or deployment.

## Private branch synchronization: 13 November 2026 UTC

An isolated copy was reviewed and revalidated on review/single-row-header-2026-11-13. Fresh runs passed: 23 community-profile checks, 21 serial simulations, 64 hidden header cases and hidden dialog construction checks. A Release build and source-hygiene check were rerun. This branch records the implementation for review; main is not changed by this source synchronization. Earlier local/uncommitted wording above describes the original handoff, not this branch's publication state. All hardware/full-app/physical-DPI limitations remain. No user settings, live source or machinery was accessed.
