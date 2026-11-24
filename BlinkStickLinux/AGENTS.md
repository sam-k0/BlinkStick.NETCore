# BlinkStickLinux

Linux GUI for controlling a BlinkStick, built on **Avalonia 11** (Fluent theme). .NET 10 console app (`OutputType Exe`) — it opens an Avalonia window via the classic desktop lifetime.

## Overview

Single window (`MainWindow : Window`) with:
- A `ColorPicker` control (from the `Avalonia.Controls.ColorPicker` package) for selecting the color.
- A live preview swatch (rounded `Border`) showing the current color's hex + RGB text, with contrasting foreground.
- A "Set Color" button → `blinkstick.SetColorAll(0, ...)`; the button's background is filled with the current color.
- Preset swatch buttons (Red, Green, Blue, Yellow, Purple, Cyan, White, Off) in a `WrapPanel`; each is filled with its color and, on click, syncs the picker/preview then applies.
- "Save Config" — persists the current picker color as the startup color.
- A `ToggleSwitch` for "Turn off on exit" — persists `TurnOffOnExit` in the config.
- On window close (`OnClosing`): if `TurnOffOnExit` is set, LEDs are turned off, then `blinkstick.Shutdown()`.

The device is always a `BlinkStickFlex` (32 LEDs) — this project hardcodes the Flex model.

## UI structure (XAML)

The UI is defined in **Avalonia XAML** (`.axaml` files), compiled by the Avalonia SDK automatically — no special csproj entries needed:
- `App.axaml` — `<Application>` with `<FluentTheme />` in `Application.Styles`.
- `MainWindow.axaml` — the whole window layout. Named controls use `x:Name` (the source generator emits the matching fields); preset buttons carry their RGB values in the `Tag` attribute (e.g. `Tag="255,0,0"`) and share one `Click="Preset_Click"` handler.

Code-behind follows the standard Avalonia pattern:
- `App.axaml.cs` — `App : Application`; `Initialize()` calls `AvaloniaXamlLoader.Load(this)`; creates `MainWindow` in `OnFrameworkInitializationCompleted`.
- `MainWindow.axaml.cs` — `InitializeComponent()` (generated), event wiring, `UpdatePreview()`, and all device/config logic.
- `Program.cs` — thin entry point: `AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args)`.

## Key files

- `MainWindow.axaml` / `MainWindow.axaml.cs` — window UI and behavior.
- `App.axaml` / `App.axaml.cs` — application setup.
- `Program.cs` — `Main` / `BuildAvaloniaApp()`.
- `Config.cs` — `Config` model (`StartupColor` tuple, nullable `TurnOffOnExit`) and `ConfigWriter`, which reads/writes `config.json` **next to the executable** (not next to the source) via Newtonsoft.Json. Defaults: red startup, turn off on exit.
- `PluginLoader.cs` — defines an `ILEDPlugin` interface (`Initialize`/`Animate`/`End`). It is **not used anywhere** — it's a stub for a planned plugin/effect system.
- `icon.ico` — application icon, referenced from the .csproj.

## Gotchas

- **Namespace matters for XAML**: all XAML-backed classes (`App`, `MainWindow`, `Program`) live in the `BlinkStickLinux` namespace. The Avalonia XAML source generator derives the generated name-scope file's namespace from `x:Class` — a class in the global namespace produces an empty `namespace { }` declaration and a CS1001 build error. (`Config.cs`/`PluginLoader.cs` are still in the global namespace, which C# name lookup resolves fine from the `BlinkStickLinux` namespace.)
- HID access needs root/udev. The `BlinkStickFlex` constructor swallows a missing device (prints it), so the app starts fine but `SetColorAll` calls throw "Device not initialized" if the device isn't reachable.
- `configWriter.loadedConfig` is a non-nullable field, but several code paths guard against null inconsistently.
- `GetApplicationPath()` uses `Process.GetCurrentProcess().MainModule.FileName`; running via `dotnet run` makes that the dotnet host, which can break config file location expectations.
- Avalonia's `Color(byte, byte, byte, byte)` constructor is **A-first** (`a, r, g, b`). Always build colors with `Color.FromArgb(255, r, g, b)` (as `MakeColor` does) — passing `(r, g, b, 255)` silently rotates the channels (red→blue, green/blue become transparent).
- The `Avalonia.Desktop` meta-package is required alongside `Avalonia` for the classic desktop lifetime to start; `.UsePlatformDetect()` on the `AppBuilder` is also required.
- There is no project reference cycle: this project references `..\BlinkStickNETCore\BlinkStickNETCore.csproj`.

## Build & test

```bash
dotnet build BlinkStickLinux.csproj
```

No unit tests.
