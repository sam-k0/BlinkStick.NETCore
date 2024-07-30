using System.Runtime.CompilerServices;
using Regex = System.Text.RegularExpressions.Regex;

public class ArgumentParser
{

    // argument list with get method
    public List<Argument> ValidArguments {  get; private set;} = new List<Argument>();

    /**
    Arguments:
    -c, --color: Set the color of the LED: Allowed values: [0-255, 0-255, 0-255], whitespaces optional between values
    -h, --help: Show help message: allowed values(one of): all, color, help, about
    **/

    public enum ArgumentType
    {
        Color,
        Help,
        Sudo,
        Shutdown
    }

    public struct Argument{
        public string option;
        public ArgumentType type;
        public string value;

        public Argument(string option, ArgumentType type, string value)
        {
            this.option = option;
            this.type = type;
            this.value = value;
        }
    }

    Dictionary<string, string> argumentFormat = new Dictionary<string, string>
    {
        {"-c", @"\b(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\s*,\s*(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\s*,\s*(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\b"},
        {"--color", @"\b(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\s*,\s*(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\s*,\s*(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\b"},
        {"-h", "all|color|help|about"},
        {"--help", "all|color|help|about"},
        {"--sudo", "true|false"},
        // shutdown can be any string
        {"--shutdown", ".*"}
    };


    

    public ArgumentParser(string[] args)
    {
        // Check if the argument list is valid: Each option must have a value except for the shutdown command
        if (args.Length % 2 != 0)
        {
            throw new ArgumentException("Invalid argument list");
        }

        // Check if the arguments are in the Dictionary of options
        for (int i = 0; i < args.Length; i += 2)
        {
            if (!argumentFormat.ContainsKey(args[i]))
            {
                throw new ArgumentException($"Invalid argument (err1): {args[i]}");
            }

            // Check if the value of the argument is valid
            if (!Regex.IsMatch(args[i + 1], argumentFormat[args[i]]))
            {
                throw new ArgumentException($"Invalid value for argument: {args[i]}");
            }
        }

        // add them to the validPairs list
        for (int i = 0; i < args.Length; i += 2)
        {
            //parse option to ArgumentType
            ArgumentType type = args[i] switch
            {
                "-c" => ArgumentType.Color,
                "--color" => ArgumentType.Color,
                "-h" => ArgumentType.Help,
                "--help" => ArgumentType.Help,
                "--sudo" => ArgumentType.Sudo,
                "--shutdown" => ArgumentType.Shutdown,

                _ => throw new ArgumentException($"Invalid argument (err2): {args[i]}")
            };

            ValidArguments.Add(new Argument(args[i], type, args[i + 1]));
        }
    }
}