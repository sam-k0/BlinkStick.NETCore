# BlinkStickNETCore

Cross-platform .NET 10 library for talking to BlinkStick hardware (Flex / Mini / Strip) over USB HID.

## Overview

All three device classes share the same HID vendor/product ID (`0x20a0` / `0x41e5`) and differ only in `NUM_LEDS`:

| Class | `NUM_LEDS` |
|---|---|
| `BlinkStickFlex` | 32 |
| `BlinkStickMini` | 4 |
| `BlinkStickStrip` | 8 |

Because the PID is identical across models, the library cannot detect which device is plugged in — the caller picks the class (and therefore the LED count) itself.

## Architecture

- `ControllerBase` — abstract base: owns the `HidApi.Device` handle, builds HID control messages, `SetColor`, `GetNumLeds`, `DeviceValid`, `DeviceProductName`, `Shutdown`.
- `BlinkStickFlex` / `BlinkStickMini` / `BlinkStickStrip` — concrete subclasses, each with a no-arg constructor that opens the HID device and a `SetColorAll` helper that loops over all LEDs.
- HID command format (built in `ControllerBase.BuildControlMessage`): 6-byte packet `[0x05, channel, index, R, G, B]`.

## Conventions & gotchas

- Namespace is `BlinkStickNETCore`; classes use file-scoped namespaces and collection expressions (`[...]`) — match the existing style.
- Colors are always 3-byte RGB arrays, channel is always `0`.
- `BlinkStickFlex`'s constructor swallows a missing device (prints and leaves `_device` null); `Mini`/`Strip` throw. `SetColor` then fails later with a generic "Device not initialized". Use `DeviceValid()` to check before use.
- Errors are plain `Exception` with string messages — no custom exception types.
- `Shutdown()` calls `Hid.Exit()`, which tears down the HID library process-wide, not just this instance.
- The `Effects` project in the repo root is an experimental playground that references this library — not part of the library itself.

## Build & test

```bash
dotnet build BlinkStickNETCore.csproj
```

There are no unit tests in this project (and none in the repo at all). Hardware is required to run anything against a real device; USB access on Linux generally needs root or a udev rule.
