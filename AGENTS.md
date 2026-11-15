# C2C DNC engineering workflow

Read README.md, CONTRIBUTING.md, SECURITY.md and docs/ENGINEERING.md before changing code. Read .specify/memory/constitution.md and the applicable specs/ documents when working through Spec Kit.

- Keep changes small and preserve unrelated work. Develop on a branch; do not push or merge without the task's authorization.
- Write acceptance criteria and a proportionate risk note before implementation. State intended behavior and behavior that must remain unchanged.
- Preserve the serial service and text processing unless a separately scoped change explicitly authorizes changes. Unverified starter profiles live in the repository, not bundled defaults; preserve saved user configurations. Never infer permission for live-machine transfers from development permission.
- Use synthetic fixtures and disconnected/simulated tests first. Simulation is not electrical, driver, flow-control or CNC validation.
- Keep credentials, licensing issuer tools/keys, real NC programs, customer data, runtime state and build outputs out of Git. Review staged content and any history before sharing.
- Validate the final source with the documented build and simulation commands. Review the final diff and conduct a deliberate second review. Record passed, failed, skipped and untested checks honestly.
- Keep source, release packages and private runtime data separate. Preserve existing editions and user settings.
- Use least-privilege CI with no privileged secrets for pull requests. Review dependency changes and provenance.
- Record version/commit, environment, commands, results, limitations and rollback. A successful push is not a test pass.
- C2C-owned code and starter profiles use MIT; preserve third-party licenses. Publication, new collaborator access, credential changes and machine operations require separate task authorization; these engineering instructions alone do not authorize them.
