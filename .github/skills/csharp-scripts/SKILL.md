---
name: csharp-scripts
description: 'Run single-file C# programs as scripts for quick experimentation, prototyping, and concept testing. Use when the user wants to validate a small C# idea without changing the main Shos.MakeItDown project.'
argument-hint: 'Run a small C# experiment outside the main project'
user-invocable: true
---

# C# Scripts

This repository includes a local adaptation of the Microsoft .NET Skill for C# scripts.

## When To Use
- Test a small C# idea before integrating it into the CLI project.
- Validate .NET 10 or C# language behavior quickly.
- Prototype parsing, path handling, or small utility logic outside the main project.

## When Not To Use
- The work belongs directly in the existing solution.
- The task needs multiple project files, project references, or production code structure.
- The user asked to change the application itself rather than run an isolated experiment.

## Repository-Specific Guidance
- Keep script files outside the existing project directory to avoid interference with `.csproj` evaluation.
- Prefer this skill only for short-lived experiments.
- Move proven logic into the real project instead of leaving important behavior in scripts.

## Procedure
1. Confirm the task is exploratory and does not require modifying the production project.
2. Create a single `.cs` file outside project folders.
3. Use top-level statements.
4. Run the file with the .NET 10 SDK.
5. Remove the script after the experiment or translate the result into production code.

## References
- [Official skill notes](./references/official-notes.md)
