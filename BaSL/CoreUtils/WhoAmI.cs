using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class WhoAmI : App, IHelpProvider
{

    public WhoAmI(ExecutableContext context) : base(context)
    {
    }

    public async Task DisplayHelpAsync(CancellationToken cancellationToken)
        => await StandardOutput.WriteLineAsync("Prints the current user's name to stdout.", cancellationToken);

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync(UserContext.Name, cancellationToken);
        return 0;
    }

}
