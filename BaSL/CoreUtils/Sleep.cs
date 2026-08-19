using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.CoreUtils;

[Help("Sleeps for the given amount of seconds (incredibly inaccurate for some reason).")]
public sealed partial class Sleep : App
{

    public Sleep(ExecutableContext context) : base(context)
    {
    }

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
