# Profile library acceptance and risk

- Version `0.9.8-open-preview.2`: fresh installs start with no installed or selected machine profiles. Catalog availability must not silently install, select or activate a setup.
- Fifteen historical configurations remain repository-only Not confirmed starter downloads under `profiles/starters`. Transfer values are unchanged; personal COM identifiers are omitted.
- Three setups (EMCO/Siemens 810T, Yasnac MX-3 and Haas Classic) are available in the catalog with `UserVerified` status based on owner-reported hardware experience. This is setup-specific reported verification, not `IndependentlyConfirmed`. They require explicit review, a local name and COM port, and Add before installation; they are not selected or installed at first launch.
- External imports require review, a local name and port, then explicit Add. Never auto-download, auto-connect or transfer.
- Existing persisted profiles and edited legacy defaults survive upgrades as local unconfirmed profiles. Do not overwrite private settings.
- Header single-row commit e281dfa and serial/text-processing behavior remain unchanged.
- Main risks: empty UI, lost legacy profile data, false trust labels. Tests cover these with temporary paths, synthetic evidence and fake serial endpoints. No CNC validation claimed.
