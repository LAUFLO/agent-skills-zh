# Project Quality Rules

## Scope

- Project: `{{PROJECT_NAME}}`
- Stack: `{{STACK}}`
- These rules apply to the repository unless a deeper `AGENTS.md` overrides them.

## Encoding

- Read and write text as UTF-8 unless BOM, metadata, or an explicit requirement says otherwise.
- Preserve the repository's configured line endings.
- If text is garbled, retry UTF-8 before guessing another encoding.

## Structure

- Keep business source out of the repository root unless the framework convention requires it.
- Put application code, tests, documentation, assets, scripts, generated files, and build outputs in distinct locations.
- Preserve dependency direction and avoid circular references.
- Do not edit vendor, dependency, generated, cache, or build-output directories.

## File Size

- Review files above 500 lines for mixed responsibilities.
- Split files above 800 lines unless they are generated, declarative, migrations, snapshots, or have a documented reason.
- Split by responsibility and dependency boundary, not mechanically by line count.

## Changes

- Inspect existing instructions, Git status, build files, and tests before editing.
- Preserve unrelated user changes.
- Update imports, project manifests, build scripts, tests, and documentation when moving files.
- Add a regression test for defect fixes when practical.

## Validation

- Format/lint: `{{FORMAT_COMMAND}}`
- Static analysis: `{{ANALYZE_COMMAND}}`
- Tests: `{{TEST_COMMAND}}`
- Build: `{{BUILD_COMMAND}}`
- Report only checks that actually ran.

## Git Authority

- Code modification, commit, push, PR, merge, and release are separate authorization levels.
- Do not commit, push, merge, tag, or publish unless the user explicitly requests that level.
- Never stage unrelated changes or use destructive Git commands to remove user work.
