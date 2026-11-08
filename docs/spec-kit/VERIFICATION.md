# Spec Kit integration evidence

Date: 2026-11-08. Environment: Windows AMD64 (10.0.26300), Python 3.13.14. Scope: development scaffold only; application build/test evidence belongs to the owning repository workstream.

## Provenance

The GitHub release endpoint reported v1.3.2, published 2026-11-06. Its annotated tag object `bd4bd60aa94b1b867afba76c36e311990d9ea591` resolves to commit `55264c2f2f2d1146f02abc543cb913dcdf1f9342`. Pinned upstream README, installation and Codex integration docs were inspected. Installation used official PyPI package `specify-cli==1.3.2` in the ignored repository-local virtual environment.

The published wheel `specify_cli-1.3.2-py3-none-any.whl` was separately downloaded and its SHA-256 verified against PyPI release metadata:

`66ea3a8460aefef6cae78a646c0374ed6ab94631e7db2e270e4737fb13902163`

This verifies the downloaded wheel identity, not a claim of a full supply-chain or vulnerability audit. The installed package reports 1.3.2. Exact installed dependency versions are recorded in `requirements-win-py313.txt`. `scaffold-sha256.txt` records the initial generated skills, scripts, templates and workflow hashes. The upstream MIT license was retrieved at the pinned commit and retained in `third_party/spec-kit/LICENSE`.

## Passed

- Supported `init --here --force --non-interactive --integration codex --integration-options='--skills --events false' --script ps --ignore-agent-tools` completed successfully using bundled assets.
- `specify version`: 1.3.2.
- `python -m pip check`: no broken requirements.
- `specify integration list`: Codex installed and default.
- All nine generated Codex `SKILL.md` files present; all six PowerShell scripts parse without syntax errors using PowerShell's parser.
- `specify workflow list` and `workflow info speckit`: bundled Full SDD Cycle 1.0.1 with specification and plan review gates.
- `workflow run --help`: documented runner accepts source and `--input key=value` inputs.
- With `SPECIFY_FEATURE_DIRECTORY=specs/001-open-preview`, `check-prerequisites.ps1 -Json -RequireSpec -RequireTasks -IncludeTasks` succeeded and found tasks.md.
- `.specify/integration.json` records `events=false`; no `.codex/config.toml` created.
- `git check-ignore` confirms `.venv-specify/` and `.specify/feature.json` are excluded.
- Scoped scan of scaffold/specs/provenance found no workstation user name or absolute user-directory references. Final full repository/staged-history checks remain the repository workstream's responsibility.

## Findings corrected during verification

- Current CLI has no `--no-git` flag. No Git extension was installed; this integration performed no Git init, commits, branches, pushes or remote creation.
- The old `SPECIFY_FEATURE` variable did not select the baseline and the prerequisite check correctly failed. Using documented current `SPECIFY_FEATURE_DIRECTORY` made it pass. Setup instructions use the verified variable.
- Current Codex integration uses `$speckit-specify` style skills under `.agents/skills`, not older `.codex/prompts` commands.

## Not run / limitations

- Codex CLI was not present on PATH during the initial check. The installed skills were inspected, but interactive skill invocation and an automated end-to-end agent workflow were not executed. Open a fresh Codex project session to discover repository skills; a separate Codex CLI is needed for automated dispatch.
- No optional extensions, catalog components, event automation, persistent access or global settings were installed.
- No application code, serial test or physical CNC transfer was performed by this scaffold workstream. No statement here certifies the preview for machine use.
- Feature tasks are intentionally unchecked until the owning workstream links application/repository evidence. Future license/public-release and hardware authorization gates remain open.

## Review and rollback

The second scoped review checked relative links, command syntax against current help, feature selection, privacy boundaries, ignored local state, retained upstream license and consistency of constitution/spec/plan/tasks. Revert scaffold/document changes independently of application changes if needed; the local ignored virtual environment is development-only. Keep working commercial installations and runtime data untouched.
