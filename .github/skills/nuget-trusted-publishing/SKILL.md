---
name: nuget-trusted-publishing
description: 'Set up NuGet trusted publishing with GitHub Actions and OIDC. Use when packaging or publishing Shos.MakeItDown as a dotnet tool or NuGet package without long-lived API keys.'
argument-hint: 'Set up or review NuGet trusted publishing'
user-invocable: true
---

# NuGet Trusted Publishing

This repository includes a local adaptation of the Microsoft .NET Skill for NuGet trusted publishing.

## When To Use
- Prepare Shos.MakeItDown for NuGet or dotnet tool publishing.
- Replace API-key-based publishing with OIDC.
- Review package metadata and publish workflow readiness.
- Create or revise a GitHub Actions workflow for release publishing.

## Repository-Specific Guidance
- Treat this project as a future dotnet tool unless the packaging strategy changes.
- Validate pack metadata before any publish workflow is added.
- Do not remove or replace publish infrastructure without explicit confirmation.

## Procedure
1. Classify the package type for this repository.
2. Check `.csproj` and any repository-wide build metadata for package settings.
3. Confirm whether the tool should be packable and what command name it should expose.
4. Draft the GitHub Actions publish workflow only after the package metadata is coherent.
5. Prefer OIDC-based NuGet publishing over API keys.
6. Require local `dotnet pack` validation before first publish.

## References
- [Official skill notes](./references/official-notes.md)
- [Package readiness checklist](./references/package-readiness.md)
