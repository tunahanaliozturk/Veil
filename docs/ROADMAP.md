# Veil Roadmap

Veil is a sensitive-data masking and PII-redaction library for .NET. The roadmap below is the
public view of where the library is going. Each phase ships as an independent release with its
own changelog entry; this document is the living index. It is a planning artifact, not a
contract — dates slip, priorities reshuffle. If an item here matters to you, open a GitHub issue
so we can weigh it against everything else.

Status legend: **Done** (shipped) · **Next** (in progress) · **Planned** (designed, target window
committed) · **Considered** (interesting but unscheduled, needs a concrete use case) ·
**Out of scope** (explicitly declined for the 1.x line).

---

## v1.x — Core masking engine

**Status:** Done

| Version | Status  | Feature                          | Details                                                                                                                                                                                              |
| ------- | ------- | -------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| v1.0    | Done    | **String & object masking**      | `Veil.Mask()`, `Veil.Redact()`, built-in patterns (Email, Phone, CreditCard, IBAN, Token, ApiKey, IPv4, TurkishId), the `[Veiled]` attribute, and `Veil.MaskObject()` with immutable-copy semantics. |
| v1.0    | Done    | **Convention-based masking**     | Attribute-free masking via `options.Convention()` — match properties by name and apply patterns automatically. Auto-detection engine for unknown input types.                                        |
| v1.0    | Done    | **HTTP middleware**              | `UseVeilRedaction()` for ASP.NET Core — header, JSON body field, and query-string redaction with conditional rules.                                                                                  |
| v1.0    | Done    | **Serilog integration**          | `Destructure.WithVeil()` for object destructuring and `Enrich.WithVeilRedaction()` for message-template redaction — zero changes to existing log calls.                                              |
| v1.0    | Done    | **Locale support**               | Region-specific pattern sets: Turkey (TC Kimlik, Vergi No, IBAN TR), Italy (Codice Fiscale, Partita IVA, IBAN IT), EU (GDPR-aware patterns).                                                          |
| v1.0    | Done    | **Custom pattern API**           | `VeilPatternDefinition` — define patterns with custom regex and mask strategy. Full control over detection and output formatting.                                                                    |
| v1.0.1  | Done    | **Repository & docs polish**     | Community-health files (CONTRIBUTING, CODE_OF_CONDUCT, SECURITY), `.github` templates, standalone roadmap, README cleanup, repository-URL fixes. No public API change.                                |

---

## v2.x — Observability & performance

**Status:** Planned

| Version | Status  | Feature                       | Details                                                                                                                                                                       |
| ------- | ------- | ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| v2.0    | Next    | **OpenTelemetry integration** | `Moongazing.Veil.OpenTelemetry` — automatic PII redaction in OpenTelemetry span attributes and trace data, so distributed traces stay compliant without manual sanitization. |
| v2.1    | Planned | **Source generator**          | Compile-time object masker built on Roslyn — zero reflection overhead, AOT-friendly, ideal for high-throughput scenarios.                                                     |
| v2.2    | Planned | **Benchmark suite**           | Comprehensive BenchmarkDotNet benchmarks, a performance-regression CI gate, and allocation optimization for masking hot paths.                                                |

---

## Considered (no commitment)

Ideas under discussion that need a concrete use case before they move to *Planned*. Raise an
issue if any of them matter to you.

- **`Microsoft.Extensions.Logging` provider** — a redaction wrapper for projects that use the
  built-in logging abstraction rather than Serilog.
- **`Moongazing.Veil.Compliance.Redaction`** — bridge to .NET 8's `Microsoft.Extensions.Compliance.Redaction`
  data-classification primitives so Veil patterns plug into the framework's redaction pipeline.
- **gRPC interceptor** — redact PII in gRPC request/response messages, mirroring the ASP.NET Core
  HTTP middleware.
- **More locales** — additional region-specific identifier sets (DE, FR, UK) driven by demand.
- **Unmasking / tokenization** — reversible masking backed by a key store, for flows that need to
  recover the original value later. Larger surface; needs a real use case.

If any of the above maps to a workload you are on right now, open an issue with the `roadmap`
label and a short description — that is how items move from *considered* to *planned*.

---

## Out of scope for the 1.x line

- **A managed redaction service.** Veil is an in-process library. Running masking as a sidecar or
  remote service is deliberately out of scope.
- **Encryption / at-rest protection.** Veil masks for display, logging, and transport. Encrypting
  data at rest is a different concern — use the appropriate data-protection / KMS tooling.
- **Full DLP / classification engine.** Veil masks known shapes of PII; it is not a heuristic
  data-loss-prevention scanner.

---

## Release cadence

| Release | Target             | Theme                                          |
| ------- | ------------------ | ---------------------------------------------- |
| v1.0.0  | shipped            | Core masking engine                            |
| v1.0.1  | shipped 2026-06-14 | Repository & docs polish                       |
| v2.0.0  | next               | OpenTelemetry integration                      |
| v2.1.0  | planned            | Source generator (compile-time masking)        |
| v2.2.0  | planned            | Benchmark suite & performance gate             |

Patch releases ship as needed for bugs and security. Minor releases cluster features around the
themes above and never break documented public APIs without a deprecation cycle. Dates are
targets, not commitments.

---

## Contributing & feedback

Issues, design discussions, and PRs are welcome on
[GitHub](https://github.com/tunahanaliozturk/Veil). See [CONTRIBUTING.md](../CONTRIBUTING.md) for
how to get started.
