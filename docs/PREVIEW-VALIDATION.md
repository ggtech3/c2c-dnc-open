# Preview application validation summary

Date: 2026-11-08. Version: 0.9.8-open-preview.1. Source baseline: 0.9.8/v28, revision 24b8fe7be3b41095339e66b053f8c58ca64b2fea. Private owner review only.

The reviewed preview removes the licensing type, issuer tooling, registration/purchase screens, trial counter and transfer allowance checks. It uses a separate application identity and LocalAppData directory. SerialDncService.cs, DncTextProcessor.cs, CncProfile.cs and Program.cs were preserved from the baseline; all application C# files in this clean repository matched the final preview candidate before Git text normalization.

Candidate validation on Windows x64 with .NET SDK 10.0.303:

- 28 checks passed against the actual packaged assembly using a temporary diagnostic hook in a copied folder with spaces and an unrelated working directory. The bundled CoreLib loaded with shared DOTNET_ROOT disabled. Checks covered absence of the licensing type/counter, ignored exhausted synthetic old state, 25 synthetic transfer completions, busy/reentrancy controls, Stop/cancel, theme/settings/restart and About tabs. Transfer delegates were fake; no serial I/O occurred. The diagnostic hook and fixtures are not included in this source snapshot or the release.
- Normal-session startup, synthetic file opening and About were inspected in the real app. A second process routed a synthetic file to the existing preview and exited successfully. The preview and original licensed application coexisted; the original window and its data were not operated or changed.
- 21 source-simulation scenarios passed separately; scope and preserved behaviors are documented in Tests/SerialSimulation/README.md.
- Independent source review found no actionable application regression. A stale profile-migration statement was corrected before the final snapshot.

Limits: same laptop, not a clean Windows installation. No real COM port, USB driver, physical handshaking or CNC validation. The copied-assembly harness is not a complete manual UI test. Broad unsaved-edit/repeated-open UX coverage remains an explicit follow-up rather than a claimed pass. Incomplete percent frames may complete on idle fallback, repeated service requests transmit repeatedly and failed transfers can be partial. These baseline behaviors are not safety assurances.

The source repository adds build metadata, locked restores and test exclusions, but does not change application C# behavior. Repository build/simulation evidence is in VERIFICATION.md. Preserve the unchanged commercial installation as rollback; no settings migration is needed.
