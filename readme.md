## Simple Blinkstick (Flex) client written in C# for Linux

Currently, only changing colors on all LEDs at once is supported.
Colors can be mixed by RGB values or presets.

Program must be run as superuser, as hidapi otherwise has insufficient access rights.

### Building

Dependencies are:
- gtksharp
- hidapi.net
- newtonsoft.json



Build release: `dotnet build -c Release`
