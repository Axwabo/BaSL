using BaSL.Executables;
using BaSL.FileSystems.Extensions;

namespace BaSL.Shell.Console;

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
            await StandardError.WriteLineAsync(result.Error.Message);
            return 1;
        }

        var directory = result.Value;
        await using var writer = StandardOutput;
        foreach (var entry in directory.EnumerateEntries())
        {
            var memory = entry.FullPath.Value.AsMemory();
            var slash = memory.Span.LastIndexOf('/') + 1;
            await writer.WriteLineAsync(memory[slash..], cancellationToken);
        }

        return 0;
    }

}
