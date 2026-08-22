using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.CoreUtils;

[Help("Prints the current user's name to stdout.")]
public sealed partial class WhoAmI : App
{

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync(UserContext.Name, cancellationToken);
        return 0;
    }

}
