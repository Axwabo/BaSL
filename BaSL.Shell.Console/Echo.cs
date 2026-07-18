using BaSL.Executables;

namespace BaSL.Shell.Console;

public sealed class Echo : App
{

    public Echo(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var args = Args;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args.Span[i];
            await StandardOutput.WriteAsync(arg);
            if (i != args.Length - 1)
                await StandardOutput.WriteAsync(' ');
        }

        return 0;
    }

}
