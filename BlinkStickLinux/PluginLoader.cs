using BlinkStickNETCore;

public interface ILEDPlugin
{
    void Initialize(ControllerBase controller);
    void Animate();
    void End();
}