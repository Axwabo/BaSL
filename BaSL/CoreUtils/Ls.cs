using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Ls : App
{

    public Ls(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = Args.Length == 0 ? WorkingDirectory : FileSystem.ResolveDirectory(Args.Span[0]);
        if (!result.Success)
        {
            await StandardError.WriteLineAsync(result.Error.Message, cancellationToken);
            return 1;
        }

        var directory = result.Value;
        await using var writer = StandardOutput;
        foreach (var entry in directory.EnumerateEntries())
            await writer.WriteLineAsync(entry.Name, cancellationToken);
        return 0;
    }

}
