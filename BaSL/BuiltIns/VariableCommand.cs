using BaSL.Executables;

namespace BaSL.BuiltIns;

internal abstract class VariableCommand : SyncCommand
{

    protected VariableCommand(ExecutableContext context) : base(context)
    {
    }

    protected sealed override void Execute()
    {
        foreach (var arg in Args)
        {
            var equals = arg.IndexOf('=');
            // TODO: proper variable name validation
            if (equals > 1 && arg[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
                Process(arg[..equals], arg[(equals + 1)..]);
        }
    }

    protected abstract void Process(string name, string value);

}
