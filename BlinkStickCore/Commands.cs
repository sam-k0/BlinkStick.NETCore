// this is hardware abstraction layer for Controller
using System;

namespace BlinkStickCore
{
    public static class Commands
    {
        public static bool SetColor(string[] arg, BlinkstickController controller)
        {
            Console.WriteLine("SetColor called");
            return true;
        }

        public static void SetColorAll(int channel, byte[] color)
        {
            Console.WriteLine("SetColorAll called");
        }

        public static void Shutdown()
        {
            Console.WriteLine("Shutdown called");
        }
    }
}
