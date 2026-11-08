# Data model
Contribution: schema version 1; public title; complete portable serial/text settings; contributor alias (optional), machine/controller, firmware/options, machine-side settings, adapter/driver, cable wiring, application version, test date, independent send/receive/drip-feed results and limitations. Required context may explicitly say unknown or not tested rather than inventing facts.

Portable settings: all transfer fields in CncProfile except local identity/port/built-in flag, free-text guidance, source URL and internal compatibility revision. Bounds match the existing editor: baud 50..921600, bits 5..8, legal serial enums, line ending CR/LF or LF or CR, pacing 0..60000 ms, nullable idle 1..3600 s, parity replacement 0..127. Idle completion requires a timeout.

Catalog: schema version, entries with stable unique ID/revision, contribution, status, maintainer/date/review notes; independent status needs separate confirming tester/date/test evidence. No runtime downloaded catalog. Zero initial entries.

Local copy: fresh GUID, IsBuiltIn=false, local name and COM port, imported settings, provenance guidance limited to historical source/revision/status and caution about edits. Existing profile store remains the persistence boundary.

Strict parse: maximum file 64 KiB, JSON maximum depth 16, duplicate/unknown members rejected, all contract members required except explicitly nullable/optional strings, bounded text and plain text display. No arbitrary metadata or executable members.
