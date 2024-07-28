

// See https://aka.ms/new-console-template for more information
Console.WriteLine("CLI parsing...");

ArgumentParser argumentParser;
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

argumentParser.ValidArguments.ForEach(arg =>
{
    Console.WriteLine($"Option: {arg.option} Value: {arg.value}");
});

return 0;
