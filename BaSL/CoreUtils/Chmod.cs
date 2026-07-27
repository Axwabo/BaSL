using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

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

        var (addOwner, addOthers, removeOwner, removeOthers) = ParseModeChange(Args.Span[0]);
        foreach (var arg in Args[1..])
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var entry = WorkingDirectory.Resolve(arg);
            if (!entry.Success)
            {
                await StandardError.WriteAsync("Cannot find ", cancellationToken);
                await StandardError.WriteAsync(arg);
                await StandardError.WriteAsync(": ", cancellationToken);
                await StandardError.WriteLineAsync(entry.Error.Message, cancellationToken);
                return 1;
            }

            var metadata = entry.Value.Metadata;
            var newOwner = (metadata.OwnerMode | addOwner) & ~removeOwner;
            var newOthers = (metadata.OthersMode | addOthers) & ~removeOthers;
            if (metadata.ChangeMode(UserContext, new Modes(newOwner, 0, newOthers)) is not { } error)
                continue;
            await StandardError.WriteAsync("Cannot change ", cancellationToken);
            await StandardError.WriteAsync(entry.Value.FullPath, cancellationToken);
            await StandardError.WriteAsync(": ", cancellationToken);
            await StandardError.WriteLineAsync(error.Message, cancellationToken);
        }

        return 0;
    }

    private (Mode AddOwner, Mode AddOthers, Mode RemoveOwner, Mode RemoveOthers) ParseModeChange(string s)
        => Modes.TryParseOctal(s, out var modes)
            ? (modes.Owner, modes.Others, ~modes.Owner, ~modes.Others)
            : Args.Span[0] switch
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
