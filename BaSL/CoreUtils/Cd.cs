using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Cd : App
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = WorkingDirectory.ResolveDirectory(Args.FirstOrDefault(UserContext.User.Home));
        if (!result.Success)
        {
            await StandardOutput.WriteLineAsync(result.Error.Message, cancellationToken);
            return 1;
        }

        Console.CurrentDirectory = result.Value;
        return 0;
    }

}
