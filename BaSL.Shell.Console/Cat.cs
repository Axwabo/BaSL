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
        for (var i = 0; i < Args.Span.Length; i++)
        {
            var arg = Args.Span[i];
            var entry = FileSystem.ResolveFile(arg);
            if (!entry.Success)
            {
                await StandardError.WriteLineAsync(entry.Error.Message);
                return 1;
            }

            var open = entry.Value.Open(UserContext, OpenMode.Read);
            if (!open.Success)
            {
                await StandardError.WriteLineAsync(open.Error.Message);
                return 1;
            }

            await using var stream = open.Value;
            await stream.CopyToAsync(StandardOutput.BaseStream, cancellationToken);
        }

        return 0;
    }

}
