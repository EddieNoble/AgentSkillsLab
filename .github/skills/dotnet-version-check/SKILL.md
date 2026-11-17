---
name: dotnet-version-check
description: Report the .NET SDK version available to the current solution. Use when asked to check, verify, or report the active dotnet SDK version.
---

# .NET SDK Version Check

1. Run `dotnet --version` from the solution root.
2. If the command cannot find an SDK, check for the Windows user-local CLI at
   `$HOME\.dotnet\dotnet.exe`. If it exists, run that executable with `--version`.
3. Report the exact version, solution directory, and which CLI path produced it.
4. If neither command can find an SDK, report the error. Do not install an SDK, change
   `PATH`, or infer the project SDK version from an IDE.

This is a read-only check. The fallback reports the SDK available through the
user-local CLI; it does not make that CLI the default for other terminal sessions.
The check does not list every installed SDK or change project files.
