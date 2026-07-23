using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class Sleep : App
{

    public Sleep(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            return 1;
        if (!int.TryParse(Args.Span[0], out var seconds))
            return 1;
        await Task.Delay(seconds * 1000, cancellationToken);
        return 0;
    }

}
