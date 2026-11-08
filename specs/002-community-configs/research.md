# Research decisions
- **Decision:** Offline JSON contribution and embedded reviewed catalog. **Rationale:** Works without accounts and cannot silently update machine setup. **Alternatives:** Hosted intake and signed online catalog deferred pending publication/terms decisions.
- **Decision:** Explicit portable DTO rather than serialize CncProfile. **Rationale:** Excludes local COM identity and hidden future fields. **Alternative:** Sanitizing arbitrary profile JSON risks unexpected data leakage.
- **Decision:** External file is unreviewed; embedded catalog may be user-reported or independently confirmed. **Rationale:** Self-asserted badges are not evidence. **Alternative:** Trust arbitrary file status rejected.
- **Decision:** New local copy only, save before UI mutation. **Rationale:** No overwrite, rollback by deleting the added custom profile. **Alternative:** Updating existing IDs creates machine-setting and data-loss risk.
- **Decision:** Empty initial catalog. **Rationale:** Existing historical claims do not supply the structured independent evidence required here.
All design questions resolved using existing architecture and agreed scope. No additional dependency or online research required.
