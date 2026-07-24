using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;

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

        var entry = WorkingDirectory.GetEntry(Args.Span[0]);
        if (entry.Value is not Directory directory)
        {
            await StandardOutput.WriteLineAsync(RemoveEntryError.NothingToRemove.Message, cancellationToken);
            return 1;
        }

        if (WorkingDirectory.FullPath.Value.StartsWith(directory.FullPath.Value))
        {
            await StandardError.WriteLineAsync("Refusing to remove current directory (or parent)", cancellationToken);
            return 1;
        }

        if (WorkingDirectory.RemoveEntry(Args.Span[0]) is not { } error)
            return 0;
        await StandardError.WriteLineAsync(error.Message, cancellationToken);
        return 1;
    }

}
