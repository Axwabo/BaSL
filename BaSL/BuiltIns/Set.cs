using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

[Help("Sets a variable in the current shell (but not for subshells). Usage: set variable=value")]
internal sealed partial class Set : VariableCommand
{

    public Set(ExecutableContext context) : base(context)
    {
    }

    protected override Task Process(string name, string value, CancellationToken cancellationToken)
    {
         Local[name] = value;
         return Task.CompletedTask;
    }

}
