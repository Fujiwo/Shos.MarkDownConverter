---
name: dotnet-pinvoke
description: 'Correctly call native libraries from .NET using P/Invoke and LibraryImport. Use only when Shos.MakeItDown introduces true native interop; do not use this for Python process execution or MarkItDown bridging.'
argument-hint: 'Review or implement native interop safely'
user-invocable: true
---

# .NET P/Invoke

This repository includes a local adaptation of the Microsoft .NET Skill for P/Invoke.

## When To Use
- Introduce real native library interop in .NET.
- Review or debug `[DllImport]` or `[LibraryImport]` declarations.
- Diagnose native boundary failures such as access violations, bad marshaling, or handle lifetime bugs.

## When Not To Use
- Calling Python executables, scripts, or CLI tools.
- Implementing MarkItDown integration through external process execution.
- Pure managed .NET code with no native boundary.

## Repository-Specific Guidance
- MarkItDown integration in this repository is process-based, not native interop.
- Do not apply P/Invoke patterns to Python invocation.
- Only load this skill if the repository later adds genuine native dependencies.

## Procedure
1. Confirm that the dependency is a native library rather than an external process.
2. Choose `LibraryImport` for new code targeting modern .NET unless compatibility requires otherwise.
3. Make type mapping, calling convention, and ownership explicit.
4. Use `SafeHandle` for native handles.
5. Add validation for interop-specific behavior.

## References
- [Official skill notes](./references/official-notes.md)
