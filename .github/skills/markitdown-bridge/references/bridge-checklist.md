# Bridge Checklist

## Scope
- Keep conversion logic in MarkItDown.
- Keep orchestration logic in .NET.

## Design Checks
- Use System.CommandLine at the CLI boundary.
- Keep file discovery, output resolution, and exit code mapping outside the bridge.
- Avoid direct `Process` calls in high-level orchestration code.
- Prefer explicit request and result models for conversion operations.

## Environment Checks
- Verify Python is installed.
- Verify MarkItDown is installed.
- Distinguish missing runtime from conversion failure.

## Documentation Checks
- State the required Python version.
- State whether `markitdown[all]` is assumed or only selected extras.
- Distinguish upstream supported formats from repository-guaranteed formats.

## Test Checks
- Command parsing tests
- Environment probe tests
- Output path resolution tests
- Failure and exit code tests
