using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

[Help("Unsets the given variables.")]
internal sealed partial class Unset : BuiltInCommand
{

    public Unset(ExecutableContext context) : base(context)
    {
    }

    public override Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            Local.Remove(arg);
            Exported.Remove(arg);
        }

        return Task.FromResult(0);
    }

}
