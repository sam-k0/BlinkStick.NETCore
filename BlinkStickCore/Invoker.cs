public class Invoker
{
    // function pointer prototype
    public delegate void FunctionPointer(string[] args);
    public Dictionary<ArgumentParser.ArgumentType, FunctionPointer> functions = new();
    public Invoker()
    {}
    public void AddFunction(ArgumentParser.ArgumentType type, FunctionPointer function)
    {
        if (functions.ContainsKey(type)) // check if the function already exists
        {
            return;
        }
        functions.Add(type, function);
    }

    public int InvokeMethod(ArgumentParser.Argument arg)
    {
        if (functions.ContainsKey(arg.type))
        {
            functions[arg.type](arg.value.Split(",")); // call the function with the arguments
            return 0;
        }

        return 0;
    }

}