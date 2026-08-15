using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class Sleep : App, IHelpProvider
{

    public Sleep(ExecutableContext context) : base(context)
    {
    }

    public async Task DisplayHelpAsync(CancellationToken cancellationToken)
        => await StandardOutput.WriteLineAsync("Sleeps for the given amount of seconds (incredibly inaccurate for some reason).", cancellationToken);

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            return 1;
        if (!int.TryParse(Args[0], out var seconds))
            return 1;
        await Task.Delay(seconds * 1000, cancellationToken);
        return 0;
    }

}
