using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
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

        if (recursive)
            return await RemoveRecursive(path, french, cancellationToken);
        var entry = WorkingDirectory.Resolve(path);
        if (entry.Value is not File)
        {
            await StandardOutput.WriteLineAsync(RemoveEntryError.NothingToRemove.Message, cancellationToken);
            return 1;
        }

        if (WorkingDirectory.RemoveEntry(Args.Span[0]) is not { } error)
            return 0;
        await StandardError.WriteLineAsync(error.Message, cancellationToken);
        return 1;
    }

    private async Task<int> RemoveRecursive(string path, bool french, CancellationToken cancellationToken)
    {
        var resolve = WorkingDirectory.Resolve(path);
        if (!resolve.Success)
        {
            if (french)
                return 0;
            await CannotRemoveAsync(path, resolve.Error, cancellationToken);
            return 1;
        }

        if (resolve.Value is not Directory directory)
        {
            var file = resolve.Value;
            return await RemoveOneAsync(file, french, cancellationToken);
        }

        var entries = directory.EnumerateEntriesRecursive().ToList();
        entries.Reverse();
        foreach (var entry in entries)
            await RemoveEntryAsync(entry, french, cancellationToken);
        await RemoveEntryAsync(resolve.Value, french, cancellationToken);
        return 0;
    }

    private async Task RemoveEntryAsync(FileSystemEntry entry, bool french, CancellationToken cancellationToken)
    {
        var parent = entry.GetParent();
        if (!parent.Success)
        {
            if (!french)
                await CannotRemoveAsync(entry.FullPath.Value, parent.Error, cancellationToken);
            return;
        }

        var remove = parent.Value.RemoveEntry(entry.Name);
        if (remove is not null && !french)
            await CannotRemoveAsync(entry.FullPath.Value, remove, cancellationToken);
    }

    private async Task CannotRemoveAsync(string path, FileSystemError error, CancellationToken cancellationToken)
    {
        await StandardError.WriteAsync("Cannot remove ", cancellationToken);
        await StandardError.WriteAsync(path, cancellationToken);
        await StandardError.WriteAsync(": ", cancellationToken);
        await StandardError.WriteLineAsync(error.Message, cancellationToken);
    }

    private async Task<int> RemoveOneAsync(FileSystemEntry entry, bool french, CancellationToken cancellationToken)
    {
        var parent = entry.GetParent();
        if (!parent.Success)
        {
            if (french)
                return 0;
            await CannotRemoveAsync(entry.FullPath.Value, parent.Error, cancellationToken);
            return 1;
        }

        var removeFile = parent.Value.RemoveEntry(entry.Name);
        if (removeFile is null)
            return 0;
        await CannotRemoveAsync(entry.FullPath.Value, removeFile, cancellationToken);
        return 1;
    }

}
