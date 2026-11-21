using BlinkStickNETCore;

public class BlinkstickController
{
    private readonly BlinkStickFlex _blinkstick;

    public BlinkstickController()
    {
        _blinkstick = new BlinkStickFlex();
    }

    public bool DeviceValid()
    {
        return _blinkstick.DeviceValid();
    }

    public string? DeviceProductName()
    {
        return _blinkstick.DeviceProductName();
    }

    public void SetColor(int channel, int num, byte[] color)
    {
        _blinkstick.SetColor(channel, num, color);
    }

    public void SetColorAll(int channel, byte[] color)
    {
        _blinkstick.SetColorAll(channel, color);
    }

    public void Shutdown()
    {
        _blinkstick.Shutdown();
    }
}
