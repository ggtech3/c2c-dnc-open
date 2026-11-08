# Spec Kit development workflow

This repository uses official [GitHub Spec Kit](https://github.com/github/spec-kit), pinned to **1.3.2** (released 2026-11-06), with Codex skills and Windows PowerShell scripts. It is development tooling only; the DNC application remains an offline .NET application and does not require Python.

## What is included

- `.agents/skills/speckit-*`: nine generated Codex skills: constitution, specify, clarify, plan, checklist, tasks, analyze, implement and converge.
- `.specify/scripts/powershell/`: six upstream helper scripts.
- `.specify/templates/`: five upstream artifact templates.
- `.specify/workflows/speckit/workflow.yml`: bundled Full SDD Cycle 1.0.1 with review gates.
- `.specify/memory/constitution.md`: project-specific engineering principles.
- `specs/001-open-preview/`: initial spec, plan, tasks and requirements review.

Native Codex event handlers are disabled (`--events false`). No optional Git, GitHub, bug, assessment or agent-context extension was installed. No global Codex settings or credentials were changed. Spec Kit 1.3.2 makes Git integration optional; initialization does not require the old `--no-git` flag, which this version does not accept.

## Restore the local CLI

Run from the repository root in PowerShell with Python 3.13 on Windows x64:

```powershell
python -m venv .venv-specify
.\.venv-specify\Scripts\python.exe -m pip install -r .\docs\spec-kit\requirements-win-py313.txt
.\.venv-specify\Scripts\specify.exe version
.\.venv-specify\Scripts\python.exe -m pip check
```

The dependency snapshot pins the installed versions for the verified Windows/Python environment; it is not a cross-platform lock or a security audit. `.venv-specify/` is ignored and must not be committed. This uses the upstream-supported PyPI route (`specify-cli==1.3.2`); no global installation or PATH modification is necessary. An independent Python environment can instead install the single exact CLI version, but transitive versions may then differ.

The tracked scaffold is already initialized. **Do not rerun initialization over project edits routinely.** To reproduce it in a new/scratch project, the actual command used was:

```powershell
.\.venv-specify\Scripts\specify.exe init --here --force --non-interactive --integration codex --integration-options='--skills --events false' --script ps --ignore-agent-tools
```

`--force` permits merging/overwriting scaffold files. Back up and review changes before any regeneration. `--ignore-agent-tools` skipped a Codex CLI executable check; this does not verify that the optional workflow runner can launch Codex.

## Use with Codex

Open this repository in a fresh Codex session so its `.agents/skills` are discovered. In Codex chat, invoke **dollar-prefixed hyphenated skills**, not the historical `/speckit.*` prompts:

```text
$speckit-constitution Review the existing C2C DNC constitution; preserve all machine-safety and private-release boundaries.
$speckit-specify Describe the next authorized feature and its acceptance criteria.
$speckit-clarify
$speckit-plan Use the existing offline Windows WinForms .NET 10 architecture and preserve serial behavior.
$speckit-tasks
$speckit-analyze
$speckit-implement
$speckit-converge
```

Run and review each phase separately. Start by reviewing existing baseline artifacts rather than regenerating them or repeating already completed application changes. An agent's generated tasks cannot authorize public release, credentials or physical CNC transfers. Mark tasks complete only with actual evidence.

For the existing baseline, select its directory in the shell/session used by the helpers (1.3.2 uses `SPECIFY_FEATURE_DIRECTORY`, not the older `SPECIFY_FEATURE` variable):

```powershell
$env:SPECIFY_FEATURE_DIRECTORY = 'specs/001-open-preview'
& .\.specify\scripts\powershell\check-prerequisites.ps1 -Json -RequireSpec -RequireTasks -IncludeTasks
```

The helper records local feature selection in ignored `.specify/feature.json`. Selection is local state; do not commit absolute local paths. Use a normal development branch under the repository contribution policy; no Spec Kit Git automation is installed.

The bundled automation can be inspected with `specify workflow list`, `specify workflow info speckit` and `specify workflow run --help` using the local executable path. Automated agent dispatch requires a separately available `codex` CLI. No end-to-end agent automation was run during scaffold installation; manual reviewed skill use is the intended starting workflow.

## Provenance and licensing

- Upstream release: https://github.com/github/spec-kit/releases/tag/v1.3.2
- Tag resolved to commit: `55264c2f2f2d1146f02abc543cb913dcdf1f9342`.
- Official package: https://pypi.org/project/specify-cli/1.3.2/
- Pinned installation docs: https://github.com/github/spec-kit/blob/55264c2f2f2d1146f02abc543cb913dcdf1f9342/docs/installation.md
- Pinned Codex docs: https://github.com/github/spec-kit/blob/55264c2f2f2d1146f02abc543cb913dcdf1f9342/docs/integrations/codex.md
- Upstream MIT license is preserved in [third_party/spec-kit/LICENSE](../third_party/spec-kit/LICENSE).

Generated upstream skills, scripts, templates and workflow retain upstream behavior; the project constitution and feature documents are local authored content. The Spec Kit MIT license applies to Spec Kit only. It does **not** license the C2C DNC application or authorize public publication. The owner will choose the application's license separately.

## Verification boundary

Installation and validation performed 2026-11-08 on Windows AMD64, Python 3.13.14. See [verification](spec-kit/VERIFICATION.md) for exact checks and package integrity metadata. Application validation is tracked in the repository's own evidence and is not certified by this scaffold.
