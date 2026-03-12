---
name: markitdown-bridge
description: 'Use when implementing, reviewing, or refactoring Shos.MakeItDown MarkItDown integration, Python environment checks, external process execution, or supported-format decisions.'
argument-hint: 'Implement or review MarkItDown bridge behavior'
user-invocable: true
---

# MarkItDown Bridge

## When To Use
- Implement or change MarkItDown invocation.
- Add Python environment diagnostics.
- Review supported format handling.
- Refactor process execution, logging, or exit code mapping around conversion.

## Goals
- Keep Shos.MakeItDown as a .NET CLI wrapper around MarkItDown.
- Isolate Python and process execution details behind testable abstractions.
- Preserve a clear boundary between CLI orchestration and document conversion.

## Procedure
1. Confirm whether the task changes CLI behavior, environment probing, or only the bridge implementation.
2. Keep MarkItDown integration behind an interface such as `IMarkItDownRunner` or an equivalent abstraction.
3. Prefer invoking installed `markitdown` or a controlled Python bridge instead of embedding conversion logic in .NET.
4. Handle stdout as conversion output and stderr as diagnostics.
5. Map failures to stable exit codes and actionable error messages.
6. Update user-facing docs when supported formats or prerequisites change.
7. Add or update tests for orchestration logic rather than re-testing MarkItDown conversion fidelity.

## Checks
- Is Python availability validated before conversion starts?
- Is MarkItDown presence validated with a dedicated doctor-style path when possible?
- Are optional dependencies documented if a format depends on them?
- Is external process execution abstracted so tests can fake it?

## References
- [Bridge checklist](./references/bridge-checklist.md)
