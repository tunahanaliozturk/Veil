<!--
  One conceptual change per PR. Conventional Commits style title (feat:, fix:, docs:, chore:...).
  For anything beyond a typo or one-line fix, please link an issue you opened first.
-->

## What & why

<!-- What does this change, and what problem does it solve? Link the issue: Closes #123 -->

## Type of change

- [ ] Bug fix (non-breaking)
- [ ] New feature (pattern / locale / integration)
- [ ] Documentation / chore
- [ ] Breaking change

## Checklist

- [ ] Tests added/updated, and `dotnet test` passes locally.
- [ ] Masked output is asserted exactly in tests (not just "it changed").
- [ ] This change never makes output *less* masked by default (or it is flagged as breaking).
- [ ] Public API additions have XML doc comments.
- [ ] CHANGELOG.md updated for user-visible changes.
- [ ] No `Co-Authored-By` trailers.
