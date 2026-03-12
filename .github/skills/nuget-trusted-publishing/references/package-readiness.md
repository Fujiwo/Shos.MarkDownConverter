# Package Readiness

## Before Adding Publish Automation
- Decide whether Shos.MakeItDown is published as a dotnet tool, package, or both.
- Set `PackageId` and `Version` explicitly.
- If publishing as a dotnet tool, set `PackAsTool` and `ToolCommandName`.
- Ensure README packaging behavior matches the chosen distribution model.

## Local Validation
- Run `dotnet pack -c Release`.
- If packaged as a tool, install it from the local package output and run help.
- Verify package metadata and included files before touching nuget.org.
