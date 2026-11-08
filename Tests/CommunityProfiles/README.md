# Community configuration checks

Acceptance: portable transfer settings round-trip for all 18 unchanged defaults; private local fields are excluded; malformed or unsupported input fails closed; imported copies have fresh identities; catalog confirmation needs independent evidence. Synthetic fixtures only. The harness links production sources and uses the same locked System.IO.Ports dependency as the existing simulator. It never constructs a serial port or reads the real profile store.

Risk: untrusted settings can affect a later machine transfer. These checks validate parsing, conversion and evidence rules, not CNC compatibility, electrical wiring, drivers, flow control or controller acceptance. UI persistence and cancellation require separate review/disconnected GUI checks.

Run from the repository root: `pwsh -File ./Tests/CommunityProfiles/Run.ps1`.

Coverage includes all 18 defaults and all 18 writable transfer settings; boundary values and nullable percent-frame timeout; allowlisted privacy; every required member; unknown/duplicate/null/malformed data; enum/type/range/text/depth/UTF-8 size rejection; temporary-file reads; repeated identities; injected persistence success/failure and collision rejection; shipped empty resource; catalog status, independent evidence and identity validation.

The persistence callback checks save-before-return without using ProfileRepository or LocalAppData. Temporary fixtures are generated under a unique system-temp directory and removed in `finally`. Existing profile objects are compared before and after success and failure. Hidden-profile preferences and modal cancellation/busy gating are UI concerns reviewed separately, not simulated by this harness. A test pass does not validate a real write failure in the Windows profile directory or a connected CNC.
