
using System.Diagnostics;
using System.Runtime.InteropServices;
#region Global Definitions
ArgumentParser argumentParser;
Invoker invoker = new();


bool IsSuperuser()
{
    return (argumentParser.ValidArguments.Exists(arg => arg.type == ArgumentParser.ArgumentType.Sudo) && argumentParser.ValidArguments.Find(arg => arg.type == ArgumentParser.ArgumentType.Sudo).value == "true") ||
     (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && (Environment.GetEnvironmentVariable("SUDO_USER") != null));
}
#endregion

string path = Environment.GetCommandLineArgs()[0];
string executablePath = Path.GetDirectoryName(path) + "/BlinkStickCore";


// get the args
try
{
    argumentParser = new ArgumentParser(args);
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);
    return -1;
}

// Get args to pass to the executable
var executableArguments = argumentParser.ValidArguments.Aggregate("", (acc, arg) =>
{
    string argName = arg.option.ToLower();
    string argValue = arg.value;
    return acc + $"{argName} {argValue} ";
});

//System.Console.WriteLine("Executable path: {0}", executablePath);
//System.Console.WriteLine("Executable arguments: {0}", executableArguments);

// add functions to the invoker
invoker.AddFunction(ArgumentParser.ArgumentType.Color, BlinkStickCore.Commands.SetColor);
invoker.AddFunction(ArgumentParser.ArgumentType.Help, BlinkStickCore.Commands.Help);
invoker.AddFunction(ArgumentParser.ArgumentType.Sudo, BlinkStickCore.Commands.Sudo);



// Either run the executable as root or actually run the functions
if (!IsSuperuser())
{
    Console.WriteLine("You are not running as root. HidApi requires root permissions to access the USB device.");
    Console.WriteLine("Please allow running as root using pkexec by entering sudo credentials when prompted.");
    // add the sudo argument
    executableArguments += "--sudo true";

    var procInfo = new ProcessStartInfo("pkexec", $"{executablePath} {executableArguments}")
    {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    try {
        var process = Process.Start(procInfo);
        if (process == null)
        {
            Console.WriteLine("Failed to start process");
            return -1;
        }
        process.WaitForExit();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
        Console.WriteLine(process.StandardError.ReadToEnd());
    } catch (Exception e) {
        Console.WriteLine(e.Message);
        return -1;
    }
}
else
{
    // Get a controller
    BlinkstickController controller = new BlinkstickController();
    if (controller.DeviceValid())
    {
        Console.WriteLine($"Found device: {controller.DeviceProductName()}");
    }
    else
    {
        Console.WriteLine("Device is not valid.");
        return -1;
    }

    // invoke the functions
    argumentParser.ValidArguments.ForEach(arg =>
    {
        invoker.InvokeMethod(arg, controller);
    });

}


return 0;
