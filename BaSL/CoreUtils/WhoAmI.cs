using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class WhoAmI : App
{

    public WhoAmI(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync(UserContext.Name, cancellationToken);
        return 0;
    }

}
