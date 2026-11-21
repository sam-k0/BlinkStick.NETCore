namespace BlinkStickNETCore;
using HidApi;

public class BlinkStickFlex : ControllerBase
{
    protected override ushort VendorId => 0x20a0;
    protected override ushort ProductId => 0x41e5;
    protected override ushort NUM_LEDS => 32;
    

    public BlinkStickFlex()
    {

        try
        {
            _device = new Device(VendorId, ProductId);
        }
        catch (Exception ex) 
        {
            System.Console.WriteLine(ex);
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