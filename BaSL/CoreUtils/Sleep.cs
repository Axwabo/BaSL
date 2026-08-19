using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.CoreUtils;

[Help("Sleeps for the given amount of seconds (incredibly inaccurate for some reason).")]
public sealed partial class Sleep : App
{

    [Execute]
    private static async Task<int> SleepAsync(int? seconds, CancellationToken cancellationToken)
    {
        if (seconds == null)
            return 1;
        await Task.Delay(seconds.Value * 1000, cancellationToken);
        return 0;
    }

    public Sleep(ExecutableContext context) : base(context)
    {
    }

    /*public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            return 1;
        if (!int.TryParse(Args[0], out var seconds))
            return 1;
        await Task.Delay(seconds * 1000, cancellationToken);
        return 0;
    }*/

}
