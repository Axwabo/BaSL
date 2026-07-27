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

        var addOwner = Mode.None;
        var addOthers = Mode.None;
        var removeOwner = Mode.None;
        var removeOthers = Mode.None;
        switch (Args.Span[0])
        {
            case "+x":
                addOthers = addOwner = Mode.Execute;
                break;
            case "-x":
                removeOthers = removeOwner = Mode.Execute;
                break;
        }

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

}
