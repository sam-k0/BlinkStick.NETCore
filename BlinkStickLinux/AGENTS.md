# BlinkStickLinux

Linux GUI for controlling a BlinkStick, built on **Avalonia 11** (Fluent theme). .NET 10 console app (`OutputType Exe`) — it opens an Avalonia window via the classic desktop lifetime.

## Overview

Single window (`MainWindow : Window` in `Program.cs`) with:
- R/G/B `Slider` controls (0–255), each with a label and a live value readout.
- A live preview swatch (rounded `Border`) showing the current color's hex + RGB text, with contrasting foreground.
- A "Set Color" button → `blinkstick.SetColorAll(0, ...)`; the button's background is filled with the current color.
- Preset swatch buttons (Red, Green, Blue, Yellow, Purple, Cyan, White, Off) in a `WrapPanel`; each is filled with its color and, on click, syncs the sliders/preview then applies.
- "Save Config" — persists the current slider values as the startup color.
- A `ToggleSwitch` for "Turn off on exit" — persists `TurnOffOnExit` in the config.
- On window close (`OnClosing`): if `TurnOffOnExit` is set, LEDs are turned off, then `blinkstick.Shutdown()`.

The device is always a `BlinkStickFlex` (32 LEDs) — this project hardcodes the Flex model.

The `App` class (`Program.cs`) registers `FluentTheme` and creates `MainWindow` in `OnFrameworkInitializationCompleted`.

## Key files

- `Program.cs` — `MainWindow` (all UI is built in code; there are no XAML files in this project) and `App`.
- `Config.cs` — `Config` model (`StartupColor` tuple, nullable `TurnOffOnExit`) and `ConfigWriter`, which reads/writes `config.json` **next to the executable** (not next to the source) via Newtonsoft.Json. Defaults: red startup, turn off on exit.
- `PluginLoader.cs` — defines an `ILEDPlugin` interface (`Initialize`/`Animate`/`End`). It is **not used anywhere** — it's a stub for a planned plugin/effect system.
- `icon.ico` — application icon, referenced from the .csproj.

## Gotchas

- HID access needs root/udev. The `BlinkStickFlex` constructor swallows a missing device (prints it), so the app starts fine but `SetColorAll` calls throw "Device not initialized" if the device isn't reachable.
- `configWriter.loadedConfig` is a non-nullable field, but several code paths guard against null inconsistently.
- `GetApplicationPath()` uses `Process.GetCurrentProcess().MainModule.FileName`; running via `dotnet run` makes that the dotnet host, which can break config file location expectations.
- Avalonia's `Color(byte, byte, byte, byte)` constructor is **A-first** (`a, r, g, b`). Always build colors with `Color.FromArgb(255, r, g, b)` (as `MakeColor` does) — passing `(r, g, b, 255)` silently rotates the channels (red→blue, green/blue become transparent).
- There is no project reference cycle: this project references `..\BlinkStickNETCore\BlinkStickNETCore.csproj`.

## Build & test

```bash
dotnet build BlinkStickLinux.csproj
```

No unit tests.
