using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL;

public sealed class Ls : Program
{

    public Ls(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync()
    {
        await using var writer = new StreamWriter(StandardOutput.AsStream());
        foreach (var entry in Console.CurrentDirectory.EnumerateEntries())
        {
            var memory = entry.FullPath.Value.AsMemory();
            var slash = memory.Span.LastIndexOf('/') + 1;
            await writer.WriteLineAsync(memory[slash..]);
        }

        return 0;
    }

}
