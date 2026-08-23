using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.BuiltIns;

internal abstract class VariableCommand : BuiltInCommand
{

    protected VariableCommand(ExecutableContext context) : base(context)
    {
    }

    public sealed override Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            var equals = arg.IndexOf('=');
            // TODO: proper variable name validation
            if (arg is not [>= 'A' and <= 'Z' or >= 'a' and <= 'z', ..]) 
                continue;
            if (equals >= 1)
                Process(arg[..equals], arg[(equals + 1)..]);
            else
                Process(arg);
        }

        return Task.FromResult(0);
    }

    protected abstract void Process(string name);

    protected abstract void Process(string name, string value);

}
