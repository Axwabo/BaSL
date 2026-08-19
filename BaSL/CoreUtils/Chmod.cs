using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

using ModeDeltas = (Mode AddOwner, Mode AddOthers, Mode RemoveOwner, Mode RemoveOthers);

public sealed class Chmod : App
{

    public Chmod(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length < 2)
        {
            await StandardOutput.WriteLineAsync("Not enough arguments", cancellationToken);
            return 1;
        }

        var tuple = ParseModeChange(Args[0]);
        var recursive = Args[1] is "-R";
        foreach (var arg in Args[(recursive ? 2 : 1)..])
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var entry = WorkingDirectory.Resolve(arg, false);
            if (!entry.Success)
            {
                await StandardError.WriteAsync("Cannot find ", cancellationToken);
                await StandardError.WriteAsync(arg);
                await StandardError.WriteAsync(": ", cancellationToken);
                await StandardError.WriteLineAsync(entry.Error.Message, cancellationToken);
                return 1;
            }

            await ChangeModeAsync(entry.Value, tuple, cancellationToken);
            if (!recursive || entry.Value is not Directory directory)
                continue;
            foreach (var child in directory.EnumerateEntriesRecursive())
                await ChangeModeAsync(child, tuple, cancellationToken);
        }

        return 0;
    }

    private async Task ChangeModeAsync(FileSystemEntry entry, ModeDeltas tuple, CancellationToken cancellationToken)
    {
        var metadata = entry.Metadata;
        var newOwner = (metadata.OwnerMode | tuple.AddOwner) & ~tuple.RemoveOwner;
        var newOthers = (metadata.OthersMode | tuple.AddOthers) & ~tuple.RemoveOthers;
        if (metadata.ChangeMode(UserContext, new Modes(newOwner, 0, newOthers)) is not { } error)
            return;
        await StandardError.WriteAsync("Cannot change ", cancellationToken);
        await StandardError.WriteAsync(entry.FullPath, cancellationToken);
        await StandardError.WriteAsync(": ", cancellationToken);
        await StandardError.WriteLineAsync(error.Message, cancellationToken);
    }

    private ModeDeltas ParseModeChange(string s)
        => Modes.TryParseOctal(s, out var modes)
            ? (modes.Owner, modes.Others, ~modes.Owner, ~modes.Others)
            : Args[0] switch
            {
                // TODO
                "+x" => (Mode.Execute, Mode.Execute, 0, 0),
                "-x" => (0, Mode.Execute, 0, Mode.Execute),
                "+w" => (Mode.Write, Mode.Write, 0, 0),
                "-w" => (0, Mode.Write, 0, Mode.Write),
                "+r" => (Mode.Read, Mode.Read, 0, 0),
                "-r" => (0, Mode.Read, 0, Mode.Read),
                _ => (0, 0, 0, 0)
            };

}
