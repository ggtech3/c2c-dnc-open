# Engineering gates

For each change, record acceptance criteria, canonical source, scope, risk and preserved behavior. Keep business logic separate from UI, storage and services where practical. Prefer supported frameworks and validate boundaries.

Use synthetic fixtures. Exercise empty, invalid, boundary, cancellation, repeat, restart and interrupted flows proportionately. For serial work explicitly cover framing, disconnects, partial transfers, timeouts, duplicates and integrity. Development authorization is not permission to connect to or operate machinery.

Review the final diff, generated/configuration files, dependencies and staged content. Conduct a deliberate second pass and seek independent review for machine-facing or security-sensitive changes. Do not weaken tests to obtain a pass.

Keep credentials, issuer tools, real customer files, runtime state and build outputs out of Git. New publication must include a review of the complete proposed history. The initial snapshot has deliberately fresh history rather than importing the commercial repository.

Keep application source, user-facing packages and private runtime data separate. Verify release copies in a path with spaces, keep dependencies/notices together, and distinguish same-host checks from clean-machine verification. Preserve earlier releases and user settings.

Record commit, environment, commands, actual results, limitations and rollback. Use explicit passed/failed/skipped/untested labels. Block release for exposed secrets, exploitable access failures, unintended data loss or critical acceptance failures. Do not claim the whole app is secure, fully tested or machine-compatible based on CI.

## Change evidence

- Intended/preserved behavior and acceptance:
- Source, branch, version/commit and environment:
- Exposure and consequences:
- Commands and actual results:
- Final diff and second review:
- Dependencies, licensing and privacy checks:
- Packaging/data separation if applicable:
- Untested areas, mitigation and owner:
- Rollback:
- Local/committed/pushed state and verified remote SHA:
