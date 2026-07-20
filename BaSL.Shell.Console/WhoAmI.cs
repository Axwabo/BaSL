using BaSL.Executables;

namespace BaSL.Shell.Console;

public sealed class WhoAmI : App
{

    public WhoAmI(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync(UserContext.Name);
        return 0;
    }

}
