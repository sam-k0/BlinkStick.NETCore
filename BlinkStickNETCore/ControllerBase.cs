namespace BlinkStickNETCore;
public abstract class ControllerBase {

    protected HidApi.Device? _device;
    protected abstract ushort VendorId {get;}
    protected abstract ushort ProductId {get;}
    protected abstract ushort NUM_LEDS {get;}
    
    protected byte[] BuildControlMessage(int channel, int index, byte[] color)
    {
        byte[] msg = [0x05, (byte)channel, (byte)index, color[0], color[1], color[2]];
        return msg;
    }

    public void SetColor(int channel, int num, byte[] color)
    {
        if (num >= NUM_LEDS)
        {
            throw new Exception("Index out of range");
        }
        if (_device == null)
        {
            throw new Exception("Device not initialized");
        }

        byte[] msg = BuildControlMessage(channel, num, color);
        _device.Write(msg);
    }

    public abstract void Shutdown();
    public ushort GetNumLeds()
    {
        return NUM_LEDS;
    }

}