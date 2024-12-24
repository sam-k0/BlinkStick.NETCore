using System.Threading.Channels;
using HidApi;
public class BlinkstickController
{
    private const int VendorId = 0x20a0;
    private const int ProductId = 0x41e5;
    private const int MSG_PACKET_SIZE = 6;

    private const int NUM_LEDS = 32;
    private HidApi.Device _device;

    public BlinkstickController()
    {
        _device = new Device(VendorId, ProductId);
        Console.WriteLine(_device.GetManufacturer());
    }

    static byte[] BuildControlMessage(int channel, int index, byte[] color)
    {
        byte[] msg = [0x05, (byte)channel, (byte)index, color[0], color[1], color[2]];
        return msg;
    }

    public void SetColor(int channel, int num, byte[] color)
    {
        byte[] msg = BuildControlMessage(channel, num, color);
        _device.Write(msg);
    }

    public void SetColorAll(int channel, byte[] color)
    {
        for (int i = 0; i < NUM_LEDS; i++)
        {
            SetColor(channel, i, color);
        }
    }

    public void Shutdown()
    {
        Hid.Exit();
    }

}