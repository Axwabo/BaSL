using BaSL.Executables;
using BaSL.FileSystems.Extensions;
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
        var result = FileSystem.ResolveDirectory(final);
        if (!result.Success)
        {
            await StandardOutput.WriteLineAsync(result.Error.Message);
            return 1;
        }

        Console.CurrentDirectory = result.Value;
        return 0;
    }

}
