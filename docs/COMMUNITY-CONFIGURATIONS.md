# Community configurations (private preview)

This first version lets a machinist share one portable setup and try a shared setup without replacing working profiles. It is offline: the app does not upload files, create issues, check for online catalog updates, change controller parameters or open a serial port during sharing/import.

The repository is still private. Public intake, contribution/redistribution terms, website distribution and an online catalog update service require separate owner decisions. Do not invite outside contributions or publish submitted information until those decisions are made.

## Share your setup

1. Select the working profile and choose **Share setup**.
2. Give the contribution a public title. The form starts fresh instead of copying your private profile name or notes.
3. Describe the machine and exact controller, firmware/options, machine-side communication settings, RS-232 adapter/driver, and cable wiring. Say **Unknown** when information is genuinely unavailable. Do not infer settings from a similar machine.
4. Record the application version, test date and separate results for sending to memory, receiving and drip-feeding. **Not tested** is a valid and important result. Describe limitations and any failures. A software simulation is not a successful machine test.
5. Preview the entire contribution. Remove machine serial numbers, customer/shop names, account details, credentials, file paths, production programs and any other private information from the text you entered.
6. Save the small JSON contribution file. Nothing is submitted automatically. Manually send only the reviewed contribution file to the maintainer through your existing private contact method. If you have authorized repository access, the private configuration-submission issue template can organize the review.

The export contains portable transfer settings and the sharing details you enter. It does not copy the local COM assignment, internal profile ID, built-in flag, profile name, existing guidance/source URL, programs, logs, hidden-profile preferences or other profiles. Deliberately entered free text is included, so the preview is still essential.

Do not send your complete profiles.json file or screenshots containing unrelated private data. The contribution file is not a backup of your installation.

## Import a shared setup

1. Choose **Community configurations**, then open the contribution file.
2. Review the complete communication settings, machine-side instructions, cable description and test results. A file from another person is an **unreviewed user report**, not a certification, even if its text claims otherwise.
3. Enter a local profile name and the COM port assigned on your own PC. Confirm controller settings, RS-232 voltage levels, cable wiring, framing and protocol independently before any later connection.
4. Choose **Add profile** to save a new custom copy. Cancel or close to leave your profiles unchanged.

Import always creates a new identity. It does not overwrite a built-in or custom profile, change hidden-profile choices, or start a transfer. Re-importing creates another copy rather than updating your working setup. You can edit or delete an imported custom profile using the existing profile controls. Its provenance note describes the original contribution; it is not a promise that edited local settings are confirmed.

An unsupported version, invalid field, unexpected member or oversized file is rejected before import. Do not rename or bypass rejected input to force it into the app. Ask the maintainer for a supported, reviewed contribution instead.

## Understand the catalog labels

- **Unreviewed user report:** A contribution opened from a file. Test results are claims by its contributor.
- **User-reported (maintainer reviewed):** A bundled catalog entry has passed the maintainer's data/privacy/scope review. The reported machine test has not been independently confirmed.
- **Independently confirmed:** The bundled catalog records a distinct tester, date and concrete confirmation evidence in addition to maintainer review. The evidence must identify which operations were actually tested. Untested operations remain untested.

No label guarantees compatibility or safe operation on another machine. Matching a controller family is not enough. Maintainer review is not a physical test, and passing automated application checks never earns independent confirmation.

The initial catalog is intentionally empty. Existing 18 built-in presets remain unchanged and are not retroactively awarded these statuses. Reviewed entries can be bundled with a future application build. Individually shared contributions can be imported now. There is no automatic download or silent replacement of local profiles.

## Maintainer review procedure

1. Confirm submission through an authorized private channel and ensure the contributor intended to share the exact file with you. Do not treat receipt as permission for public redistribution.
2. Review every text field for private/customer data, unsafe instructions, unsupported claims and material outside communication setup. Ask the contributor to remove unnecessary information. Never commit real NC programs, serial numbers, logs or runtime state.
3. Validate with the application's strict contribution parser and inspect the app preview. Confirm schema support, settings, controller/machine, firmware/options, adapter/driver, wiring, dates, app version, send/receive/drip-feed results and limitations. Ask for missing context; do not fill it by guessing.
4. Compare against existing entries to avoid duplicates. Assign a stable catalog ID and positive revision. Preserve the contribution as reviewed. Include maintainer alias, review date and a concise explanation of what was checked.
5. Default to **UserReported**. Use **IndependentlyConfirmed** only when a distinct tester has separately tested the stated setup and supplied a date and concrete evidence. Record exact operation results, controller/firmware, cable/adapter and version, and retain limitations. Merely repeating the first person's report is not independent confirmation.
6. Add the data entry to Assets/community-configurations.json in a scoped branch, run Tests/CommunityProfiles/Run.ps1 plus the ordinary engineering checks, and obtain another review. The automated tests load and validate the actual shipped catalog but do not validate the hardware claims.
7. Do not merge, publish or change access without authorization. Before any public catalog, settle contribution/redistribution terms and confirm permission to publish the supplied attribution and setup details.

Bad or outdated entries should be corrected or withdrawn in a reviewed catalog revision. Do not silently rewrite users' imported copies. Notify affected users only through an authorized communication process. A future online-update feature must address authenticity, version compatibility, rollback and withdrawals separately.

## Data and verification

The versioned packet is an allowlisted JSON contract, not executable code. Contribution files have a 64 KiB maximum and a depth limit of 16. Unknown/duplicate fields, missing required values, invalid enums and out-of-range transfer settings are rejected. See [the feature contract](../specs/002-community-configs/contracts/contribution.md) and [validation evidence](../specs/002-community-configs/verification.md).

Run the synthetic checks from the repository root:

```powershell
pwsh -File Tests/CommunityProfiles/Run.ps1
```

These checks do not connect to a COM port. No physical adapter, wiring, driver, electrical compatibility, flow control or CNC behavior is validated by them.
