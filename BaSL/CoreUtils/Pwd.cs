using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class Pwd : App
{

    public Pwd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync(WorkingDirectory.FullPath.Value, cancellationToken);
        return 0;
    }

}
