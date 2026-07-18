using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL;

public sealed class Ls : Program
{

    public Ls(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var writer = StandardOutput;
        foreach (var entry in Console.CurrentDirectory.EnumerateEntries())
        {
            var memory = entry.FullPath.Value.AsMemory();
            var slash = memory.Span.LastIndexOf('/') + 1;
            await writer.WriteLineAsync(memory[slash..], cancellationToken);
        }

        return 0;
    }

}
