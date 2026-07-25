using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using Directory = BaSL.FileSystems.Directory;
using File = BaSL.FileSystems.File;

namespace BaSL.CoreUtils;

public sealed class Ls : App
{

    private static async Task WriteAsync(StreamWriter writer, Mode mode)
    {
        await writer.WriteAsync(mode.CanRead ? 'r' : '-');
        await writer.WriteAsync(mode.CanWrite ? 'w' : '-');
        await writer.WriteAsync(mode.CanExecute ? 'x' : '-');
    }

    public Ls(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var longFormat = false;
        string? path = null;
        foreach (var arg in Args)
            if (!longFormat && arg is "-l")
                longFormat = true;
            else
                path = arg;
        var result = path == null ? WorkingDirectory : WorkingDirectory.ResolveDirectory(path);
        if (!result.Success)
        {
            await StandardError.WriteLineAsync(result.Error.Message, cancellationToken);
            return 1;
        }

        var directory = result.Value;
        await using var writer = StandardOutput;
        foreach (var entry in directory.EnumerateEntries().OrderBy(e => e.Name.Value))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            if (!longFormat)
            {
                await writer.WriteLineAsync(entry.Name, cancellationToken);
                continue;
            }

            await writer.WriteAsync(entry is Directory ? 'd' : '-');
            var (owner, group, others) = entry.Metadata.Modes;
            await WriteAsync(writer, owner);
            await WriteAsync(writer, group);
            await WriteAsync(writer, others);
            await writer.WriteAsync(". ", cancellationToken);
            await writer.WriteAsync(entry.Metadata.Owner.Username, cancellationToken);
            await writer.WriteAsync(' ');
            await writer.WriteAsync(((entry as File)?.SizeBytes ?? 0).ToString(), cancellationToken);
            await writer.WriteAsync(' ');
            var quote = entry.Name.Value.Contains(' ');
            if (quote)
                await writer.WriteAsync('\'');
            await writer.WriteAsync(entry.Name, cancellationToken);
            if (quote)
                await writer.WriteLineAsync('\'');
            else
                await writer.WriteLineAsync();
        }

        return 0;
    }

}
