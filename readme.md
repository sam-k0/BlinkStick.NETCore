## BlinkStick Utilities

- BlinkStickNETCore: Cross-platform .NET Core library for BlinkStick (Flex, Mini, Strip)
- BlinkStickLinux: Linux GUI using Gtk# for controlling BlinkStick devices
- BlinkStickCLI: CLI for controlling BlinkStick devices

## BlinkStickNETCore

Usage:
```csharp
using BlinkStickNETCore;

BlinkStickFlex bs = new BlinkStickFlex(); // Get your blinkstick device

bs.SetColor(0,0,[0xFF,0x00,0x00]); // Set color of first LED to red

var num = bs.GetNumLeds(); // Get number of LEDs
for (int i = 0; i < num; i++)
{
    bs.SetColor(0,i,[0xFF,0x00,0x00]); // Set color of all LEDs to red
}

```



### Building

Dependencies are:
- gtksharp (GUI)
- hidapi.net
- newtonsoft.json (GUI)



Build release: `dotnet build -c Release`
