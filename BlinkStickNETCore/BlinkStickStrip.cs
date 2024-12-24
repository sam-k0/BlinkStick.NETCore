﻿namespace BlinkStickNETCore;
using HidApi;

public class BlinkStickStrip : ControllerBase
{
    protected override ushort VendorId => 0x20a0;
    protected override ushort ProductId => 0x41e5;
    protected override int NUM_LEDS => 8;

    public BlinkStickStrip()
    {
        _device = new Device(VendorId, ProductId);
        if (_device == null)
        {
            throw new Exception("Device not found");
        }
    }

    public void SetColorAll(int channel, byte[] color)
    {
        for (int i = 0; i < NUM_LEDS; i++)
        {
            SetColor(channel, i, color);
        }
    }

    public override void Shutdown()
    {
        Hid.Exit();
    }

}