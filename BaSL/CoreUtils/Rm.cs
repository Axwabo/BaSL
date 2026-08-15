using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Rm : App, IHelpProvider
{

    public Rm(ExecutableContext context) : base(context)
    {
    }

    public async Task DisplayHelpAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync("Removes a file or multiple files.", cancellationToken);
        await StandardOutput.WriteLineAsync("Use the -f flag to continue on error.", cancellationToken);
        await StandardOutput.WriteLineAsync("Use the -r flag to recurse into subdirectories.", cancellationToken);
        await StandardOutput.WriteLineAsync("Examples:", cancellationToken);
        await StandardOutput.WriteLineAsync("rm amogus.txt", cancellationToken);
        await StandardOutput.WriteLineAsync("rm -r directory", cancellationToken);
        await StandardOutput.WriteLineAsync("rm -rf /", cancellationToken);
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

        foreach (var error in entry.Value.Remove(UserContext, recursive, french))
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
