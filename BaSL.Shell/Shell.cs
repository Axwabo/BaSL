using System.IO;
using System.Threading.Tasks;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.Shell;

public sealed class Shell
{

    private readonly FileSystem _fileSystem;
    private readonly Stream _standardInput;
    private readonly Stream _standardOutput;
    private readonly Stream _standardError;

    // TODO: refactor!!1!1!1
    public Shell(Stream standardInput, Stream standardOutput, Stream standardError)
    {
        _fileSystem = FileSystem.CreateVirtual();
        _standardInput = standardInput;
        _standardOutput = standardOutput;
        _standardError = standardError;
    }

    public async Task<int> Execute()
    {
        await using var writer = new StreamWriter(_standardOutput, null!, -1, true);
        foreach (var entry in _fileSystem.Root.EnumerateEntriesRecursive())
            writer.WriteLine(entry.FullPath);
        return 0;
    }

}
