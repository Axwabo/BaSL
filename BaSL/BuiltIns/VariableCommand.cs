using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.BuiltIns;

internal abstract class VariableCommand : BuiltInCommand
{

    protected VariableCommand(ExecutableContext context) : base(context)
    {
    }

    protected string Arg { get; set; } = "";

    protected int ExitCode { get; set; }

    public sealed override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            Arg = arg;
            var equals = arg.IndexOf('=');
            // TODO: proper variable name validation
            if (equals >= 1 && arg[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z')
                await Process(arg[..equals], arg[(equals + 1)..], cancellationToken);
        }

        return ExitCode;
    }

    protected abstract Task Process(string name, string value, CancellationToken cancellationToken);

}
