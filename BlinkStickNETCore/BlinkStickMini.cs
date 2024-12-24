namespace BlinkStickNETCore;
using HidApi;

public class BlinkStickMini : ControllerBase
{
    protected override ushort VendorId => 0x20a0;
    protected override ushort ProductId => 0x41e5;
    protected override ushort NUM_LEDS => 4;

    public BlinkStickMini()
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