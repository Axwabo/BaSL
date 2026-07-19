using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using Directory = BaSL.FileSystems.Directory;
using Path = BaSL.FileSystems.Path;

namespace BaSL.Shell.Console;

public sealed class Cd : App
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        Path path = Args.Span[0];
        var final = path.Value.AsSpan().StartsWith('/') ? path : Path.Combine(WorkingDirectory.FullPath, path);
        if (FileSystem.Resolve(final) is not Directory directory)
        {
            await StandardOutput.WriteLineAsync("Not a directory");
            return 1;
        }

        Console.CurrentDirectory = directory;
        return 0;
    }

}
