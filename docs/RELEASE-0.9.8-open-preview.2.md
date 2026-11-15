# 0.9.8-open-preview.2

## Changes
- Three offline, owner-reported verified setups: EMCO ET342 / Siemens 810T, Yasnac MX-3 and Haas Classic Control. Verification describes the original setup, not independent confirmation or universal compatibility.
- Fresh startup has no installed/active machine or assumed COM port. A visible Choose verified setup action opens the three available profiles. Review, enter local name and port, then Add. Nothing connects or transfers during import.
- Fifteen unverified starters moved to the public repository's profiles/starters directory. Downloading and importing requires no GitHub account; posting a contribution on GitHub does.
- Existing saved profiles (including edited old built-ins) stay intact as local profiles. Their settings are not reset. No other C2C edition's store is read.
- Preserves the reviewed one-row header. MIT license for C2C-owned code/profiles, with component terms and notices included.

## Validation (Windows x64, .NET 10.0.11 runtime)
- PASS: locked dependency restore and self-contained Release publish to a path with spaces.
- PASS: 27 community/configuration checks, including exact transfer values for all18 historical fixtures,15 repository starters,3 owner-verified catalog entries, legacy storage preservation, empty startup, imports and trust separation.
- PASS: 21 fake-endpoint serial simulation cases; no real COM port opened.
- PASS: hidden production MainForm startup using isolated stores and no port enumeration; disabled transfer without a selected profile and visible first-run action; hidden light/dark dialog construction/cancel.
- PASS: 64 one-row header layout cases across themes, widths and four simulated layout scales.
- PASS: source hygiene and diff whitespace checks (limited automated checks).
- LIMIT: NuGet vulnerability feed was unavailable (NU1900); restore/build succeeded using locked cached dependencies. This is not a fresh vulnerability assessment.
- NOT TESTED: clean-machine installation, actual USB drivers, electrical/flow-control behavior, CNC transfer/operation, visible full-window manual interactions or real multi-monitor DPI transitions. Owner verification is historical user evidence, not a new test of this build.

## Rollback
Keep the older release and copy your C2CDncOpenPreview profiles/settings before upgrading. This release preserves the original saved file until a deliberate profile save. Do not overwrite another edition's runtime directory. Repository starter source and app release are separate. No auto-update or machine operation occurs.
