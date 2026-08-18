using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using Directory = BaSL.FileSystems.Directory;
using File = BaSL.FileSystems.File;

namespace BaSL.CoreUtils;

public sealed partial class Ls : App, IHelpProvider
{

    private static async Task WriteAsync(StreamWriter writer, Mode mode)
    {
        await writer.WriteAsync(mode.CanRead ? 'r' : '-');
        await writer.WriteAsync(mode.CanWrite ? 'w' : '-');
        await writer.WriteAsync(mode.CanExecute ? 'x' : '-');
    }

    private static async Task WriteNameAsync(StreamWriter writer, string name, CancellationToken cancellationToken)
    {
        var quote = name.Contains(' ');
        if (quote)
            await writer.WriteAsync('\'');
        await writer.WriteAsync(name, cancellationToken);
        if (quote)
            await writer.WriteAsync('\'');
    }

    public Ls(ExecutableContext context) : base(context)
    {
    }

    public async Task DisplayHelpAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync("Lists the contents of the directory (current directory if no path is given).", cancellationToken);
        await StandardOutput.WriteLineAsync("Use the \"-l\" flag to include permissions, owner, and file size.", cancellationToken);
        await StandardOutput.WriteLineAsync("Examples:", cancellationToken);
        await StandardOutput.WriteLineAsync("ls", cancellationToken);
        await StandardOutput.WriteLineAsync("ls /usr/bin", cancellationToken);
        await StandardOutput.WriteLineAsync("ls -l", cancellationToken);
        await StandardOutput.WriteLineAsync("ls -l /", cancellationToken);
    }

    // [Execute]
    public async Task<int> MogusAsync([Flag('l')] bool longFormat)
    {
        return 0;
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
            await WriteNameAsync(writer, entry.Name.Value, cancellationToken);
            if (entry is SymbolicLink link)
            {
                await writer.WriteAsync(" -> ", cancellationToken);
                await WriteNameAsync(writer, link.Target.Value, cancellationToken);
            }

            await writer.WriteLineAsync();
        }

        return 0;
    }

}
