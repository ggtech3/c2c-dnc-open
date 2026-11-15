# C2C DNC Open Preview Constitution

## I. Preserve machine-facing behavior
The existing serial engine and transfer settings are the preservation baseline. Fifteen former defaults are repository-only unverified starters. Three owner-reported tested setups may be offered in the catalog, without automatic installation or independent-confirmation claims. Their relocation must preserve saved user profiles. Framing, filtering, flow control, pacing, receive termination, parity handling, timeout, cancellation, disconnect and partial-send confirmation must not change incidentally. Any intended change needs an explicit spec, reviewed risk assessment and regression evidence. Do not refactor merely to adopt tooling.

## II. Keep local data and editions isolated
The preview is an offline Windows WinForms .NET 10 application with its own C2CDncOpenPreview identity and `%LOCALAPPDATA%\C2CDncOpenPreview` data directory. It must not depend on registration, activation, purchases or transfer quotas. Preserve commercial installations and their data. Runtime settings, logs, real NC programs and customer material never belong in source control.

## III. Require evidence before claims
Define acceptance and risk before changing behavior. Use synthetic, disconnected or simulator tests first, including invalid input, interruption, repetition, cancellation and persistence where relevant. Record exact commands, environment and passed/failed/skipped/untested outcomes after final edits. A build or simulated test is not physical machine validation. Live-machine transfers require separate explicit authorization and a controller-specific procedure.

## IV. Protect source and delivery boundaries
Keep secrets, private license tooling, keys, customer records, production NC files, binaries and runtime state out of this clean source repository. Scan staged content and relevant history before publication. Use supported dependency sources, version/lock records, retained third-party notices and least-privilege CI without automatic deployment. Distinguish developer checkout, distributable and private data. Preserve a known-good candidate and a reversible rollback path.

## V. Make changes reviewable and contributor-ready
Use small scoped changes linked to a specification, plan and verifiable tasks. Review source and generated/configuration changes, then conduct a deliberate second pass. Document build/test steps using relative paths and report missing coverage honestly. Do not mark a task complete based solely on scaffolding. Keep specs and implementation reconciled rather than rewriting completed work blindly.

## VI. Preserve owner decisions
The owner selected MIT for C2C-owned code and starter profiles and authorized preparation for public use. Actual publication still requires its authorized release review. Third-party open-source licenses apply to their components only. No workflow, generated task, agent instruction or successful check grants authority to publish publicly, change permissions, create credentials, send machine data or claim hardware certification.

## Development workflow
Read AGENTS.md and this constitution, write feature acceptance and risk, then specify, clarify where needed, plan, create tasks, analyze, implement scoped changes and verify. Use `specs/<number>-<short-name>/` for feature artifacts. Existing baseline work may satisfy tasks after evidence review. Record exceptions with impact, mitigation, owner and revisit trigger. Block release for secrets, unintended data loss or critical acceptance failures.

## Governance
This constitution guides engineering but does not override user authorization or stricter safety and repository policies. Amend it through a reviewed change with a reason, version bump and impact review of active specs/plans/tasks. Major versions change principles incompatibly; minor versions add substantive requirements; patches clarify wording. Each plan must check these principles before implementation and at final review. Initial version replaces the upstream placeholder; upstream templates remain unchanged.

**Version**: 1.1.0 | **Ratified**: 2026-11-08 | **Last amended**: 2026-11-15

Amendment: reflects the owner-authorized repository-only starter workflow and
MIT selection. The serial-preservation and evidence requirements remain in
force. Historical specs 001/002 describe their original baselines; for the
current starter relocation, `docs/PROFILE-LIBRARY-CHANGE.md` records the new
acceptance scope. Older references to 18 bundled defaults or private/no-license
status are historical, not current release instructions.
