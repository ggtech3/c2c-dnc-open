# Share one CNC profile

Fifteen former defaults are repository-only **unverified starter files** in
[`profiles/starters`](../profiles/starters). A fresh app starts with no profiles;
existing saved profiles and imports are retained. Three setup-specific catalog
profiles (EMCO/Siemens 810T, Yasnac MX-3 and Haas Classic) are available as
**user-reported tested**, not independently confirmed. None is installed or
selected automatically; review the settings and choose a local COM port first. This branch
prepares public use; publication and release availability are checked separately.

## Download without an account

Once the repository is public, open a starter JSON file on GitHub and choose
**Download raw file**. No GitHub account is needed to download public files or
release ZIPs. Choose **Community configurations** in the app, import the JSON,
review it, and enter your own name and COM port before choosing **Add profile**.
Import and use are offline and do not require Git, an account or sign-in.
Importing never opens a serial port or sends a program.

## Share in a few steps

1. Select the profile you want to share and choose **Share**.
2. Enter a public **Machine / controller** description, such as a model and control name. Optionally add a short note about what worked, cable/adapter details or problems. You do not need to fill out a technical questionnaire.
3. Choose **Review** and check the complete post. Remove customer/shop names, machine serial numbers, private paths and any other private information from your own text.
4. Choose **Copy and open GitHub**. Sign in if asked. In the new issue's description box, paste with **Ctrl+V**, then click **Submit new issue**. You can also paste into an existing relevant issue's comment if you are following up on that same profile.

The app does not post or upload anything. It copies the reviewed text, then opens the repository's GitHub page. Posting an issue or comment requires a GitHub account; downloading public files does not. The button cannot grant repository access. If the page is unavailable or access is missing, do not change repository visibility or send the profile somewhere else by default. **Save JSON copy** is an optional way to retain the reviewed JSON locally for an exchange you authorize.

The clipboard may be visible to other apps or Windows clipboard history/sync. Copy only information you intend to share. If copying fails, GitHub is not opened. If browser opening fails, the app explains that the text is copied but nothing is submitted; retry when the browser is available. Successful browser opening is not proof that a post was made.

## What is shared

Only one profile's portable communication settings, your public description, optional notes and application version are included. The local profile name, COM assignment, internal ID, built-in flag, existing notes/guidance/source URL, programs, logs, hidden-profile preferences and other profiles are not copied.

Missing details remain **Not supplied**, with no recorded structured test date/results. A note such as "sending worked" is your report, not automatic confirmation. Do not share the complete profiles.json file or screenshots containing unrelated private data. The optional JSON is not an installation backup.

## Reported testing and independent confirmation

- **Not confirmed:** A submitted/shared file without separate machine-test evidence. A maintainer reading or accepting it does not confirm its behavior.
- **User-reported tested:** The three owner-reported setups have specific real-machine experience behind them, but no independent-confirmation claim. Review their recorded scope and limitations; this is not a universal machine-family guarantee.
- **Confirmed:** A maintainer has reviewed a separate tester's evidence for the exact original setup. Only the operations both testers reported working are covered. Untested or failed operations are not confirmed.

No label guarantees compatibility with your machine. Saved legacy profiles and repository starters are not awarded a confirmation badge. The bundled list includes three owner-reported tested setups; those reports are not independent confirmation and do not automatically install profiles. Unconfirmed starter files stay in the repository, outside the app package. There is no automatic download or silent replacement.

## Try a shared profile

Choose **Community configurations**, open a contribution JSON file, review all settings/cautions and enter your own local profile name and COM port. **Add profile** creates a new custom copy. Cancel changes nothing. Reject unsupported or invalid files rather than trying to bypass validation.

Imports do not connect to a COM port. Confirm your machine parameters, RS-232 voltage levels, cable wiring, framing and protocol separately before later use. Imported profiles are editable local copies and do not inherit a continuing Confirmed badge; their source note is historical. Re-importing creates another copy, never an update to an existing setup.

## Maintainer procedure

1. Review one issue per new profile; use comments for questions and additional machine reports. No contributor branch or pull request is needed for submission.
2. Check the exact posted JSON and all entered text for private data, unsupported claims and material unrelated to communication setup. Check that the contributor has rights to submit the material under the project MIT License; do not publish private information. Request missing controller/cable/test context only when needed for review, not as a barrier to an initial report.
3. Save only the JSON block as a contribution file and validate it with the app. Do not execute content, follow submitted instructions blindly, or import an arbitrary GitHub page as configuration.
4. Default to Not confirmed. Confirmed requires a distinct tester, date, concrete evidence and at least one matching successful operation. Record limitations and exact setup/version. The parser validates evidence structure, not the truth of a human report. Software simulations do not count as machine tests.
5. The three owner-reported tested setups may be offered in the catalog with their user-reported status and limitations. Do not relabel them IndependentlyConfirmed without qualifying separate evidence. Reviewed catalog additions use the existing stable ID/revision and review metadata. Keep unverified candidates in the repository starter library and clearly label them Not confirmed. Run Tests/CommunityProfiles/Run.ps1 and ordinary checks, then obtain review. Internally the version-1 enum names remain UserReported and IndependentlyConfirmed for compatibility; user-facing labels must preserve the distinction between owner-reported testing and independent confirmation.
6. Do not merge, publish, change access or redistribute submissions without authorization. Preserve third-party terms, source attribution and user-customized profiles. Do not turn an imported file or a self-reported success into a public verification badge.

## Data boundary

Version-1 JSON is allowlisted data, maximum 64 KiB and depth 16. Unknown/duplicate members, missing required values, invalid enums and out-of-range settings are rejected. The short sharing form fills absent context explicitly rather than weakening validation. The GitHub URL is fixed and contains no profile text. Notes are confined to serialized JSON in a protected Markdown code block.

See the [contract](../specs/002-community-configs/contracts/contribution.md) and [verification record](../specs/002-community-configs/verification.md). All software checks use synthetic/disconnected data and establish no electrical, adapter, driver or CNC compatibility.
