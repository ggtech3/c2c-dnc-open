# Contribution contract
The first schema is a data-only JSON document, not a raw profiles.json backup. Field names and a synthetic example follow below. Unknown schema versions and fields are errors rather than migrations. Imports do not accept built-in identity, local COM assignment, paths, commands or review badges. Review information is trusted only from the embedded catalog built with the application.

UI contract (refined 2026-11-09): Share opens a fresh short form with one public machine/controller description and optional notes. Review shows the exact post before Copy and open GitHub or optional Save copy. Copy precedes opening a fixed HTTPS issue URL; no user data is placed in the URL. The user signs in if needed, pastes and submits. Clipboard failure prevents launch; launch failure does not claim submission. Cancel/Back has no sharing effect. Community configurations retains complete preview and local name/COM selection. Only Add profile commits a new copy; no serial operation occurs. Visible confirmation labels are Confirmed / Not confirmed, without changing the internal schema.

The short form maps description (maximum 120) to title and machine, controller to Not supplied separately, absent firmware/machine-side/adapter/cable/notes to Not supplied, version to the running application, testDate to empty, results to NotTested and alias to null. Optional notes (maximum 2000) replace limitations. Structured tests are not inferred from prose. Earlier full-detail schema-1 packets remain supported. The clipboard post contains static explanation and exact serialized JSON inside a fence that untrusted content cannot close. Markdown is packaging only; the import parser still accepts the JSON packet alone.

## Schema 1 field names
JSON names and enum strings are case-sensitive. Every non-optional constructor member is required, including nullable fields. Unknown/duplicate members, comments and trailing commas are rejected. Whole contribution max is 65,536 UTF-8 bytes including whitespace (file BOM counts toward file limit), depth max 16.

- Root: schemaVersion (integer 1), title (nonblank single line, max 120), settings, context.
- settings: baudRate (50..921600), dataBits (5..8), parity (None/Odd/Even/Mark/Space), stopBits (One/OnePointFive/Two), handshake (None/XOnXOff/RequestToSend/RequestToSendXOnXOff), dtrEnable, rtsEnable, newLine (literal CR+LF, LF or CR), lineDelayMilliseconds (0..60000), receivePercentFramed, receiveIdleTimeoutSeconds (null or 1..3600), stripNulls, removeWhitespace, omitParenthesisLines, omitLinesContainingParenthesis, removeReceiveControlCharacters, checkParity, parityReplaceCharacter (0..127). Flags are JSON booleans. Idle completion requires a non-null timeout.
- context: machine, controller, applicationVersion (nonblank single lines max 200); firmwareOptions, machineSideSettings, adapterDriver, cableWiring, limitations (nonblank plain text max 2000); testDate (yyyy-MM-dd, or empty only when all operations NotTested); sendResult, receiveResult, dripFeedResult (NotTested/Passed/Failed); contributorAlias (optional/null or nonblank single line max 120).

Unsupported control characters and bidirectional override/isolate marks are rejected. Names/identities are single-line. Multiline setup notes display indented beneath their field label. The preview includes readable labels and the exact JSON for review.

## Synthetic contribution example
This example has never been tested on a machine. It is documentation/test data only, not a recommended setup.

```json
{
  "schemaVersion": 1,
  "title": "Synthetic example - do not use on a machine",
  "settings": {
    "baudRate": 9600,
    "dataBits": 7,
    "parity": "Even",
    "stopBits": "Two",
    "handshake": "XOnXOff",
    "dtrEnable": true,
    "rtsEnable": true,
    "newLine": "\r\n",
    "lineDelayMilliseconds": 50,
    "receivePercentFramed": false,
    "receiveIdleTimeoutSeconds": 5,
    "stripNulls": true,
    "removeWhitespace": false,
    "omitParenthesisLines": false,
    "omitLinesContainingParenthesis": false,
    "removeReceiveControlCharacters": false,
    "checkParity": false,
    "parityReplaceCharacter": 0
  },
  "context": {
    "machine": "Synthetic example",
    "controller": "Synthetic example",
    "firmwareOptions": "Unknown",
    "machineSideSettings": "Synthetic example only; no machine parameters supplied",
    "adapterDriver": "Unknown",
    "cableWiring": "Unknown; do not connect",
    "applicationVersion": "schema-1 documentation example",
    "testDate": "",
    "sendResult": "NotTested",
    "receiveResult": "NotTested",
    "dripFeedResult": "NotTested",
    "limitations": "Synthetic example only. No hardware or machine validation.",
    "contributorAlias": null
  }
}
```

## Catalog schema
The embedded catalog root contains schemaVersion=1 and entries array. Every entry has id (lowercase alphanumeric groups separated by hyphens, max 80, case-insensitively unique), revision (positive integer), contribution (the packet above), status (UserReported or IndependentlyConfirmed), reviewer (single line max 120), reviewDate (yyyy-MM-dd), reviewNotes (max 2000) and confirmation (explicit null for UserReported).

IndependentlyConfirmed requires confirmation with tester (single line max 120, distinct from non-null contributorAlias), testDate, evidence (nonblank max 2000), sendResult, receiveResult and dripFeedResult. Confirmation requires at least one successful operation corroborating the contributor's result; failed/untested operations do not become confirmed. Review cannot precede independent testing. This is structural validation, not verification of the human claim. A maintainer must check the actual evidence.

The entire catalog has the same 64 KiB/depth bound in version 1. Larger future libraries require an explicitly reviewed format/capacity change, not silent truncation. Catalog parsing is a build/test API; file import accepts only a contribution packet and never calls catalog parsing.
