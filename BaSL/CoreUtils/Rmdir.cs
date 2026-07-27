using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Rmdir : App
{

    public Rmdir(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
        {
            await StandardOutput.WriteLineAsync("Directory must be specified", cancellationToken);
            return 1;
        }

        var entry = WorkingDirectory.ResolveDirectory(Args.Span[0]);
        if (!entry.Success)
        {
            await StandardOutput.WriteLineAsync(RemoveChildError.NothingToRemove.Message, cancellationToken);
            return 1;
        }

        if (WorkingDirectory.FullPath.Value.StartsWith(entry.Value.FullPath.Value))
        {
            await StandardError.WriteLineAsync("Refusing to remove current directory (or parent)", cancellationToken);
            return 1;
        }

        if (entry.Value.RemoveSelf(UserContext) is not { } error)
            return 0;
        await StandardError.WriteLineAsync(error.Message, cancellationToken);
        return 1;
    }

}
