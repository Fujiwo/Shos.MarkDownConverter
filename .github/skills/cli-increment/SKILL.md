---
name: cli-increment
description: 'Use when adding or reviewing Shos.MakeItDown CLI commands, options, help text, logging behavior, output path rules, or tests for incremental feature delivery.'
argument-hint: 'Add or review a CLI feature increment'
user-invocable: true
---

# CLI Increment

## When To Use
- Add a new command or option.
- Review help text, logging, or exit code behavior.
- Extend file discovery, overwrite rules, or output path logic.
- Check whether a proposed feature fits the planned phased rollout.

## Goals
- Deliver features in small, testable increments.
- Preserve a minimal first-run experience.
- Avoid adding options whose behavior is not yet defined.

## Procedure
1. Classify the change as one of: diagnostics, single-file conversion, output handling, directory processing, filtering, or extension support.
2. Confirm the feature belongs in the current implementation phase from `Prompts/Planning.md`.
3. Add only the command or option surface needed for that phase.
4. Define stdout, stderr, and exit code behavior explicitly.
5. Add tests for parsing and behavior before broadening feature scope.
6. Update README and help text if the user-facing workflow changes.

## Review Questions
- Does this option have a clear behavior for single-file and directory scenarios?
- Does overwrite behavior remain predictable?
- Is the help text specific and stable?
- Can the feature be tested without requiring real MarkItDown conversion quality checks?

## References
- [CLI checklist](./references/cli-checklist.md)
