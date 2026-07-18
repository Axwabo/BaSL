using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL;

public sealed class Echo : Program
{

    public Echo(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var writer = StandardOutput;
        var args = Args;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args.Span[i];
            await writer.WriteAsync(arg);
            if (i != args.Length - 1)
                await writer.WriteAsync(' ');
        }

        return 0;
    }

}
