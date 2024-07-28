public interface ILEDPlugin
{
    void Initialize(BlinkstickController controller);
    void Animate();
    void End();
}