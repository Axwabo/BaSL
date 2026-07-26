using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Rm : App
{

    public Rm(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var recursive = false;
        var french = false;
        string? path = null;
        foreach (var arg in Args)
            if (arg is "-r")
                recursive = true;
            else if (arg is "-f")
                french = true;
            else if (arg is "-rf" or "-fr") // remove the french language pack
                recursive = french = true;
            else
                path = arg;
        if (path == null)
        {
            await StandardOutput.WriteLineAsync("File must be specified", cancellationToken);
            return 1;
        }

        var entry = WorkingDirectory.Resolve(path);
        if (!entry.Success)
        {
            await StandardOutput.WriteLineAsync(entry.Error.Message, cancellationToken);
            return french ? 0 : 1;
        }

        foreach (var error in entry.Value.Remove(recursive, french))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            await StandardError.WriteAsync("Cannot remove ", cancellationToken);
            await StandardError.WriteAsync(error.EntryPath, cancellationToken);
            await StandardError.WriteAsync(": ", cancellationToken);
            await StandardError.WriteLineAsync(error.Message, cancellationToken);
            if (!french)
                return 1;
        }

        return 0;
    }

}
