# Security Policy

Veil exists to keep sensitive data out of places it should not be. A bug in Veil is therefore
often a security bug by definition: if a pattern fails to mask, a body is logged raw, or a
property slips through unredacted, real PII can leak into logs, traces, or API responses. We take
those reports seriously and ask you to do the same.

## Supported versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |
| < 1.0   | :x:                |

Security and correctness fixes land on the latest 1.0.x patch line. Older pre-1.0 builds are not
maintained — upgrade to the latest release.

## What counts as a security issue

Report these privately, **not** in a public issue:

- **Masking bypass** — a built-in or documented pattern that fails to mask a value it claims to
  mask (e.g. a credit card or IBAN format that passes through in the clear).
- **Data leak through an integration** — the ASP.NET Core middleware logging a raw request/response
  body, or the Serilog integration emitting an un-redacted property or message template.
- **Convention/attribute escape** — a `[Veiled]` property or a configured convention that is
  silently ignored, leaving the original value in the output.
- **Regex denial of service (ReDoS)** — a pattern whose regex can be driven to catastrophic
  backtracking by crafted input.
- Any other defect whose impact is "sensitive data ends up somewhere it should not."

Ordinary bugs (wrong masked formatting that does not leak more data, build issues, docs errors)
can go straight to the public issue tracker.

## How to report

**Do not open a public issue for a vulnerability.** Instead, use one of:

1. GitHub's [private vulnerability reporting](https://github.com/tunahanaliozturk/Veil/security/advisories/new)
   ("Report a vulnerability" on the Security tab), or
2. Email the maintainer directly. The address is in the package's NuGet metadata.

Please include:

- A minimal reproduction — the input value(s) and the configuration used.
- The actual (leaking) output vs the expected (masked) output.
- The affected package(s) and version, and the runtime (`dotnet --info`).

## What to expect

- **Acknowledgement** within 5 business days.
- An initial assessment (confirmed / needs-info / not-a-vuln) within 10 business days.
- For confirmed issues, a fix on the supported patch line and, where warranted, a published
  GitHub Security Advisory crediting you (unless you prefer to stay anonymous).

Please give us a reasonable window to ship a fix before disclosing publicly. We will keep you
updated on progress throughout.

Thank you for helping keep Veil — and the data it protects — safe.
