# BlinkStickCLI

Command-line tool for controlling a BlinkStick. .NET 10 console app.

## Architecture

This project **references `BlinkStickNETCore`** and uses `BlinkStickFlex` under the hood. `BlinkstickController` (`Controller.cs`) is a thin wrapper around a `BlinkStickFlex` instance that preserves the CLI's original API surface (`DeviceValid`, `DeviceProductName`, `SetColor`, `SetColorAll`, `Shutdown`) so the rest of the CLI code doesn't need to know about the library directly.

The CLI hardcodes the Flex model (32 LEDs) via `BlinkStickFlex`.

## How it works

Top-level statements in `Program.cs` drive everything:

1. `ArgumentParser` validates CLI arguments (pair format `--option value`).
2. If not running as root, the CLI **relaunches itself via `pkexec`** with `--sudo true` appended, then exits.
3. Running as root, it creates a `BlinkstickController` and dispatches each argument through an `Invoker` (a `Dictionary<ArgumentType, delegate>` of handlers in `Commands.cs`).

Supported options: `-c/--color R,G,B` (regex-validated 0–255 triple, sets **all** LEDs via `SetColorAll`), `-h/--help`, `--info`, `--shutdown`/`--exit` (turns LEDs off), `--sudo` (internal, only for the pkexec relaunch).

## Gotchas

- `pkexec` relaunch relies on `Environment.GetCommandLineArgs()[0] + "/BlinkStickCore"` — i.e., it assumes the binary is named `BlinkStickCore` and sits in the same directory as the CLI executable. Renaming the output binary breaks this path.
- `IsSuperuser()` on Linux is just a presence check of the `SUDO_USER` env var — it doesn't verify the process is actually root.
- Argument values are split on commas in `Invoker.InvokeMethod`, so `--color` values must be comma-separated (spaces allowed between them).
- `Commands.SetColor` always calls `controller.SetColorAll`, not a single-LED `SetColor`.
- The `BlinkStickFlex` constructor swallows a missing device; `DeviceValid()` returns false in that case and `Program.cs` handles it.
- No unit tests.

## Build & test

```bash
dotnet build BlinkStickCLI.csproj
```
