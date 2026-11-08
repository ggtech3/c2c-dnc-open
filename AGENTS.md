# C2C DNC engineering workflow

Read README.md, CONTRIBUTING.md, SECURITY.md and docs/ENGINEERING.md before changing code. Read .specify/memory/constitution.md and the applicable specs/ documents when working through Spec Kit.

- Keep changes small and preserve unrelated work. Develop on a branch; do not push or merge without the task's authorization.
- Write acceptance criteria and a proportionate risk note before implementation. State intended behavior and behavior that must remain unchanged.
- Preserve the serial service, text processing and all 18 profile defaults unless a separately scoped change explicitly authorizes them. Never infer permission for live-machine transfers from development permission.
- Use synthetic fixtures and disconnected/simulated tests first. Simulation is not electrical, driver, flow-control or CNC validation.
- Keep credentials, licensing issuer tools/keys, real NC programs, customer data, runtime state and build outputs out of Git. Review staged content and any history before sharing.
- Validate the final source with the documented build and simulation commands. Review the final diff and conduct a deliberate second review. Record passed, failed, skipped and untested checks honestly.
- Keep source, release packages and private runtime data separate. Preserve existing editions and user settings.
- Use least-privilege CI with no privileged secrets for pull requests. Review dependency changes and provenance.
- Record version/commit, environment, commands, results, limitations and rollback. A successful push is not a test pass.
- This repository is private review material. No public release, open-source license grant, new collaborator access, credential changes or machine operations are authorized by these instructions.
