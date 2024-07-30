// this is hardware abstraction layer for Controller
using System;

namespace BlinkStickCore
{
    public static class Commands
    {
        public static bool SetColor(string[] arg, BlinkstickController controller)
        {
            Console.WriteLine("SetColor called");
            if (arg.Length != 3)
            {
                Console.WriteLine("Invalid number of arguments when calling SetColor: " + arg.Length);
                return false;
            }

            controller.SetColorAll(0, new byte[] { byte.Parse(arg[0]), byte.Parse(arg[1]), byte.Parse(arg[2]) });

            return true;
        }

        public static bool SetColorAll(string[] arg, BlinkstickController controller)
        {
            Console.WriteLine("SetColorAll called");
            return true;
        }

        public static bool Shutdown(string[] arg, BlinkstickController controller)
        {
            controller.SetColorAll(0, [0, 0, 0]);
            return true;
        }

        public static bool Help(string[] arg, BlinkstickController controller)
        {
            Console.WriteLine("Help called");
            return true;
        }

        public static bool About(string[] arg, BlinkstickController controller)
        {
            Console.WriteLine("About called");
            return true;
        }

        public static bool Sudo(string[] arg, BlinkstickController controller)
        {
            // just return true. The sudo argument is only used to run the program as root
            return true;
        }


    }
}
