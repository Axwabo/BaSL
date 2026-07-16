using System.IO;
using System.Threading.Tasks;
using BaSL.FileSystems.Extensions;

namespace BaSL.Shell;

public sealed class Shell : Program
{

    public override async Task<int> ExecuteAsync()
    {
        await using var writer = new StreamWriter(StandardOutput, null!, -1, true);
        foreach (var entry in FileSystem.Root.EnumerateEntriesRecursive())
            writer.WriteLine(entry.FullPath);
        writer.WriteLine(FileSystem.Resolve("/home/user"));
        return 0;
    }

}
