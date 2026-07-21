using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.Shell.Console;

public sealed class Cat : App
{

    public Cat(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var entry = FileSystem.ResolveFile(Args.Span[0]);
        if (!entry.Success)
        {
            await StandardError.WriteLineAsync(entry.Error.Message);
            return 1;
        }

        var file = entry.Value;
        await using var stream = file.Open(UserContext, OpenMode.Read);
        await stream.CopyToAsync(StandardOutput.BaseStream, cancellationToken);
        return 0;
    }

}
