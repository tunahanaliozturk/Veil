# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned

- OpenTelemetry span attribute redaction (`Moongazing.Veil.OpenTelemetry`)
- Source generator for compile-time object masking (zero reflection)
- BenchmarkDotNet suite and performance regression CI gate

See [docs/ROADMAP.md](docs/ROADMAP.md) for the full plan.

---

## [1.0.1] - 2026-06-14

Documentation and repository polish. No public API changes — a drop-in upgrade from `1.0.0`.

### Added

- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `SECURITY.md` (responsible disclosure for masking
  bypasses and data leaks, with a supported-versions table).
- `.github` templates: issue forms (bug report, feature request), pull-request template, issue
  template config, `FUNDING.yml`, and Dependabot configuration for NuGet and GitHub Actions.
- Standalone [`docs/ROADMAP.md`](docs/ROADMAP.md) with status legend, release cadence, and
  "considered" / "out of scope" sections.
- "More from the Orion family" cross-links in the README.

### Changed

- Centralized the package version into `Directory.Build.props` and added `PackageReleaseNotes`.
- Slimmed the README roadmap to a summary that links to `docs/ROADMAP.md`, and pointed the
  Contributing section at the new community-health files.

### Fixed

- Corrected repository URLs across the README, package readmes, changelog, and project metadata
  (`tunahanaliozturk/Moongazing.Veil` → `tunahanaliozturk/Veil`, `main` → `master`).

---

## [1.0.0] - 2026-04-03

### Added

#### Moongazing.Veil (Core)

- `Veil.Mask()` — single-value masking with automatic data type detection
- `Veil.Redact()` — full-text scanning that masks all sensitive data in a string
- `Veil.MaskObject()` — reflection-based object masking that returns an immutable copy
- `[Veiled]` attribute with `VeilPattern`, `Show`, and `Position` parameters
- Built-in patterns: `Email`, `Phone`, `CreditCard`, `Iban`, `TurkishId`, `Token`, `ApiKey`, `Ipv4`, `Full`
- `SensitiveDataDetector` — auto-detection engine for unknown input types
- Convention-based masking via `options.Convention()` — attribute-free property matching
- Custom pattern API with `VeilPatternDefinition` (regex + mask strategy)
- Locale support: Turkey (TC Kimlik, Vergi No), Italy (Codice Fiscale, Partita IVA), EU (GDPR patterns)
- String extensions: `"value".Veil()`, `"text".RedactAll()`
- Object extensions: `obj.VeilProperties()`
- DI registration via `services.AddVeil()`
- Multi-target: `net8.0`, `net9.0`, `net10.0`

#### Moongazing.Veil.AspNetCore

- `UseVeilRedaction()` middleware for ASP.NET Core
- Header redaction via `RedactHeaders()`
- JSON body field redaction via `RedactBodyFields()` with JSON path support
- Query string redaction via `RedactQueryParams()`
- Conditional redaction via `RedactWhen()` predicate
- Configurable request/response logging with body truncation
- Multi-target: `net8.0`, `net9.0`, `net10.0`

#### Moongazing.Veil.Serilog

- `Destructure.WithVeil()` — automatic PII masking during Serilog object destructuring
- `Enrich.WithVeilRedaction()` — message template redaction enricher
- `VeilLogMessageRedactor` — log message scanning and sanitization
- Multi-target: `net8.0`, `net9.0`, `net10.0`

---

[Unreleased]: https://github.com/tunahanaliozturk/Veil/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/tunahanaliozturk/Veil/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/tunahanaliozturk/Veil/releases/tag/v1.0.0
