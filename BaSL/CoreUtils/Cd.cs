using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using Path = BaSL.FileSystems.Path;

namespace BaSL.CoreUtils;

public sealed class Cd : App
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var path = Args.Length == 0 ? UserContext.User.Home : Args.Span[0];
        var final = path.Value.AsSpan().StartsWith(Path.Root.Value) ? path : Path.Combine(WorkingDirectory.FullPath, path);
        var result = FileSystem.ResolveDirectory(final);
        if (!result.Success)
        {
            await StandardOutput.WriteLineAsync(result.Error.Message, cancellationToken);
            return 1;
        }

        Console.CurrentDirectory = result.Value;
        return 0;
    }

}
