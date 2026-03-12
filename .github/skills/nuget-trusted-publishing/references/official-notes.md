# Official Notes

- Source skill: Microsoft .NET Skills `plugins/dotnet/skills/nuget-trusted-publishing`
- Upstream repository: https://github.com/dotnet/skills
- This local adaptation is intended for future packaging and release automation work in this repository.

## Key Points
- Prefer `NuGet/login@v1` with `id-token: write` for nuget.org publishing.
- Validate the package locally before first publish.
- Keep workflow, package metadata, and repository documentation aligned.
