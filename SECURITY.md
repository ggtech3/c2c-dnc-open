# Security and safety

## Current status

C2C DNC Open Preview is software prepared for public use under MIT for C2C-owned material. It is not a security-certified or machine-validated release. Existing commercial editions are outside this repository's support scope.

## Report an issue privately

Do not put credentials, customer files, real NC programs, license keys or exploitable vulnerability details in a public issue. Contact the repository owner through an already-established private channel. If GitHub's private vulnerability reporting is enabled for this repository, it may be used instead. Do not assume that feature is enabled.

Include the affected version/commit, a minimal synthetic reproduction, expected and actual behavior, and impact. Remove unnecessary personal or production data. No response-time or support commitment is made here.

## Safety boundaries

- Never infer CNC compatibility from a matching profile name.
- Match controller settings, cable wiring, framing and transfer protocol before connecting.
- Simulation checks do not establish electrical, driver, hardware handshaking or machine behavior.
- Partial transfer failures need operator assessment before retrying. Do not automatically resend.
- The simulator documents preserved baseline behaviors, including idle completion of incomplete percent frames and repeated requests transmitting repeatedly. Those are not safety guarantees.
- Do not use development tools or CI to open real COM ports or operate a CNC.

Keep secrets, issuer tooling and customer/runtime data out of source. Before any public release, review staged content, complete history, dependency licenses, vulnerability reports and release artifacts. A pattern scan is only one safeguard, not proof that content is safe.
