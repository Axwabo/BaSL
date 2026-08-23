using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

[Help("Sets a variable that is also available in subshells. Usage: export variable=value")]
internal sealed partial class Export : VariableCommand
{

    public Export(ExecutableContext context) : base(context)
    {
    }

    protected override Task Process(string name, string value, CancellationToken cancellationToken)
    {
        Exported[name] = Local[name] = value;
        return Task.CompletedTask;
    }

}
