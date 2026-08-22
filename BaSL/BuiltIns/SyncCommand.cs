using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.BuiltIns;

internal abstract class SyncCommand : BuiltInCommand
{

    protected SyncCommand(ExecutableContext context) : base(context)
    {
    }

    public sealed override Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        Execute();
        return Task.FromResult(0);
    }

    protected abstract void Execute();

}
