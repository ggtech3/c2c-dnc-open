# Implementation Plan: Community configurations
**Branch**: `002-community-configs` | **Date**: 2026-11-08 | **Spec**: [spec.md](spec.md)

## Summary
Add an offline Share setup / Community configurations workflow to the existing WinForms application. Export an allowlisted versioned packet. Validate and preview external data, then add a new profile only after local name/COM selection. Bundle a reviewed catalog as an embedded resource; start empty rather than fabricate confirmation. Document private manual intake.

## Technical Context
C# / .NET 10 Windows Forms; System.Text.Json and existing System.IO.Ports dependency. No new packages or network client. Keep new transfer DTO and validation independent from UI. CncProfile defaults, SerialDncService and DncTextProcessor stay byte-identical. Tests use synthetic data and temporary directories, never the real profile store or a COM port.

## Constitution Check
Before implementation: I preserve machine-facing behavior with no engine/default edits. II preview data remains isolated. III synthetic tests and explicit limits. IV no networking, secrets, publication or executable payload. V feature spec/plan/tasks and two reviews. VI private/manual review only. All gates pass. Recheck after final diff.

## Project Structure
- CommunityConfiguration.cs: portable records, allowlist, bounds, parsing, conversion, display text.
- CommunityCatalog.cs and Assets/community-configurations.json: curated embedded entries and status/evidence validation.
- CommunityProfilesDialog.cs and ShareProfileDialog.cs: catalog/import preview and contribution form.
- MainForm.cs: entry points, transfer busy gating and save-before-list-mutation import.
- Tests/CommunityProfiles/: dependency-light test executable and Run.ps1.
- docs/COMMUNITY-CONFIGURATIONS.md and private issue template: submission/review procedure.
- specs/002-community-configs/: specification lifecycle and validation evidence.

## Design and Risk
Use a strict schema with required fields, finite enumerations, bounded UTF-8 size/depth/text and rejection of unknown/duplicate members. Portable settings exclude ID, built-in flag, COM port, guidance and source URL. Export title and sharing details are fresh deliberate inputs, not copied from private profile text. Full preview uses plain text only; no links execute. External imports are always unreviewed user reports regardless of claimed review. Trusted catalog status exists only in the reviewed embedded resource and requires evidence. Local copies get provenance text saying settings may be changed and status is not a guarantee. There is no status promotion button and no network update.

Profile persistence is staged: construct a candidate list, save it first, then replace in-memory state. Unique IDs and duplicate-safe names prevent replacements. Import/export buttons are disabled during transfer; dialogs are modal. Retain existing storage format and all existing profile fields. Empty catalog is an expected state.

## Verification Strategy
Add targeted round-trip, schema, bounds, malicious input, trust-label, duplicate identity and failure/cancellation checks. Exercise all 18 default conversions without modifying default definitions. Run locked restore, Release build, community checks, existing serial simulation, source hygiene, publish packaging and disconnected GUI checks where supported. Verify final diff and independent second review. No release, network publication, real serial operation or hardware claim.

## Dependencies and Delivery
Core data contract precedes UI/tests. Core and UI use disjoint files, then integration/testing. User docs may proceed in parallel. Final evidence reconciles tasks, outcomes and limitations. Local branch only until publication is authorized.
