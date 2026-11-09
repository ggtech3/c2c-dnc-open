# Research decisions (refined 2026-11-09)
- GitHub issue per profile, comments for follow-up. Primary flow is review/copy/open/paste/Submit with no file handling, installed Git or app-managed authentication. GitHub account and existing private-repo access remain required; no public intake is implied.
- Fixed short HTTPS issue URL, no user data in query. GitHub supports title/body parameters but warns long URLs can fail. Clipboard avoids payload URLs and makes manual submission explicit. Clipboard privacy is disclosed.
- Keep JSON export/import and version-1 DTO. A one-field public machine/controller description plus optional notes maps to explicit unprovided context and unrecorded tests; never fabricate detailed evidence from prose.
- Visible status only Confirmed / Not confirmed. Internal enum/evidence schema remains compatible. Confirmation must still refer to separately tested original settings/operations, never a generic compatibility promise.
- Keep portable allowlist, new-copy-only save-before-mutation import and empty initial catalog. Original profiles and transfer engine unchanged. No online catalog or website.

Verified official references:
- https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/creating-an-issue (issue creation requires repo read access; query parameters and URL limits)
- https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/attaching-files (manual JSON attachments supported; upload happens before final post, so not the primary simplified path)

No dependency or authentication service needed. This refines the earlier offline-only sharing UI while retaining offline import and manual submission.
