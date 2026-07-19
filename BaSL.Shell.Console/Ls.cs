using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Shell.Console;

public sealed class Ls : App
{

    public Ls(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var list = Args.Length == 0 ? WorkingDirectory : FileSystem.Resolve(Args.Span[0]);
        if (list is not Directory directory)
        {
            await StandardError.WriteLineAsync("Not a directory");
            return 1;
        }

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
