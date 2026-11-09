# Implementation Plan: Simple community profile sharing
**Branch**: `002-community-configs` | **Refined**: 2026-11-09 | **Spec**: [spec.md](spec.md)

## Summary and technical context
Refine the existing local implementation rather than replace its validated boundary. Keep C#/.NET 10 WinForms, System.Text.Json, version-1 DTO, empty embedded catalog and transactional import. No new package, authentication client or online catalog. Original presets/serial/text processing remain byte-identical.

## Design
Add a pure CommunitySharing helper with CreateQuickContribution(profile, description, notes, appVersion), BuildGitHubPost(contribution), and a fixed SubmissionUrl for ggtech3/c2c-dnc-open/issues/new. The public description becomes title and the combined machine/controller description; Controller explicitly says it was not supplied separately. Unprovided details use a literal Not supplied marker; date empty, operation results NotTested and alias null. This preserves the version-1 parser without inventing context. Required description maximum 120 characters, optional notes maximum 2000. App version is automatic.

Build one exact post with explanatory text and serialized JSON in a Markdown-safe fenced block. Untrusted text must not escape the fence. The fixed URL contains only static title parameters, never user data, a file path or payload. No dependency on an issue template that is not yet on main. Imported JSON remains usable by existing maintainers; no import of arbitrary comments or online fetching.

ShareProfileDialog becomes two fields, then privacy review with Copy and open GitHub primary, optional Save copy and Cancel/Back. It copies before browser launch, handles clipboard contention without opening, and reports launch failure without claiming posting. Explicit action is disabled/re-entry guarded during execution. Show the recipient, current private access restriction, sign-in and paste/Submit instructions. No real browser or clipboard action during automated verification.

CommunityCatalog/CommunityConfiguration display Confirmed or Not confirmed. Keep internal review enum and evidence invariants for schema compatibility. Confirmation is for original evidence only; local editable copies have no live badge. CommunityProfilesDialog displays two labels and concise help. All original profiles remain intact.

## Constitution check
I: no engine/default changes. II: data/edition isolation retained. III: synthetic checks, no hardware claims. IV: no publication or secrets; fixed explicit browser destination only. V: revised spec/plan/tasks and independent review. VI: private repo/owner decisions retained. No exceptions. Browser opening supersedes the earlier UI-only offline assumption, not automatic posting or public intake restrictions.

## Work ownership and verification
Core: CommunitySharing.cs plus CommunityConfiguration/CommunityCatalog labels. UI: ShareProfileDialog, CommunityProfilesDialog and button wording in MainForm. Tests: new helper tests, status assertions, linked compile items and no-side-effect action tests where supported. Coordinator: docs/specs/template, integration, final review and evidence.

Run locked restore, Release build, community tests, existing serial simulation, source hygiene and publish to a new ignored candidate directory. Review final diff and independent review. Do not take the active user's desktop or actually post/launch browser/copy clipboard. Record GUI paste/sign-in/repeated-click behavior as untested unless covered with isolated delegate tests. Owner subsequently authorized private push/pull/merge. Follow the draft-PR and maintainer-review workflow; do not bypass review gates or publish publicly.
