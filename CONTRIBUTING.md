# Contributing to Moongazing.Veil

Thanks for taking the time to look at this. Veil masks sensitive data and keeps PII out of logs,
HTTP traffic, and API responses for .NET. The project is small and the bar for contributions is
"does it make masking clearer, faster, or safer without expanding the public surface needlessly."

Because this is a security/privacy library, correctness matters more than features: a pattern
that *almost* masks a credit card is worse than no pattern at all.

## Before you open a PR

For anything beyond a typo, a docs tweak, or a one-line fix, please open an issue first. Five
minutes of alignment up front saves an afternoon of rework later. State:

- The use case you are trying to solve.
- What you tried that did not work.
- Whether you want to send the patch yourself or are flagging the gap.

For typos, docs polish, comment fixes, and single-line changes, skip the issue and send a PR
directly. Title it `docs: ...` or `chore: ...` so it is obvious from the queue.

## Local development

```bash
git clone https://github.com/tunahanaliozturk/Veil
cd Veil
dotnet restore
dotnet build -c Release
dotnet test
```

.NET 8 SDK is required. The multi-target build (`net8.0` / `net9.0` / `net10.0`) may need the
9.0 / 10.0 SDKs installed; the multi-target dimension is intentional and not optional.

Branch from `master`. Name the branch after intent: `feat/...`, `fix/...`, `docs/...`,
`refactor/...`, `chore/...`, `test/...`.

## Adding a pattern, locale, or integration

These are the three most common contributions. Keep them isolated:

- **New built-in pattern** — implement `IVeilPattern` in `src/Moongazing.Veil/Patterns/`, use
  `[GeneratedRegex]` (source-generated) rather than a runtime `new Regex(...)`, and add it to the
  built-in registry. Cover the happy path *and* near-miss inputs in the tests (a value one digit
  short, a value with separators, an empty string). Add a row to the "Built-in Patterns" table in
  the README.
- **New locale** — add a `VeilLocale.*` registrar under `src/Moongazing.Veil/Locales/` following
  the existing Turkey/Italy/EU registrars. Document which region-specific identifiers it covers.
- **New integration** — integrations live in their own package (`Moongazing.Veil.AspNetCore`,
  `Moongazing.Veil.Serilog`). A new sink/integration should be a new project, not bolted onto an
  existing one.

A masking change must never make output *less* masked by default. If a change can leak more than
the previous behaviour, it is a breaking change and needs a CHANGELOG note and a major/minor bump.

## Pull request shape

- One conceptual change per PR. Refactors and behaviour changes go in separate PRs even if the
  diff feels small.
- Conventional Commits style commit subject (`feat:`, `fix:`, `docs:`, etc.).
- New behaviour comes with tests. Bug fixes come with a failing-before, passing-after test.
- Public API additions need XML doc comments. Breaking changes need a CHANGELOG entry.
- No `Co-Authored-By` trailers. The author of the PR is the author of the work.

## Coding style

- The repo enforces analyzer warnings as errors (`TreatWarningsAsErrors`) and
  `latest-recommended` analysis mode. Treat warnings as bugs.
- Match the surrounding code style. If the existing code does X, do X.
- Names are spelled out. No `mgr`, `svc`, `ctx`. The exceptions are well-known abbreviations
  (`Id`, `Db`, `Url`, `Json`, `Pii`).
- Comments explain why, not what. The code already says what.
- Prefer `ReadOnlySpan<char>` and allocation-free paths in masking hot loops — this runs on every
  request and every log line.

## Tests

- xUnit + FluentAssertions.
- Test names are sentences with underscores: `Email_mask_keeps_first_and_last_char`.
- For every masking pattern, test the masked output *exactly* — assert the string, not just that
  it changed. A masking bug is invisible unless the expected output is pinned.
- Coverage is a side effect of writing tests for behaviour, not a target in itself.

## Reporting bugs

Open an issue with:

- A minimal reproduction (one DTO or one string, ideally less than 50 lines).
- The actual masked output vs the expected masked output.
- The runtime (`dotnet --info` output) and the package version.

If the bug is that **sensitive data leaks through** (a pattern fails to mask, the middleware logs
a raw body, a property is not redacted), treat it as a security issue — see below.

## Security

Do not file public issues for data-leak or masking-bypass vulnerabilities. See
[SECURITY.md](SECURITY.md) and contact the maintainer privately first.

## Conduct

Be kind. We follow the [Code of Conduct](CODE_OF_CONDUCT.md). Disagreement is fine; rudeness is
not.

## License

By submitting a pull request, you agree your contribution is licensed under the repo's
[MIT License](LICENSE).
