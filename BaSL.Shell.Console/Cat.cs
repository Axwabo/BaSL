using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using File = BaSL.FileSystems.File;

namespace BaSL.Shell.Console;

public sealed class Cat : App
{

    public Cat(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var entry = FileSystem.Resolve(Args.Span[0]);
        if (entry is not File file)
        {
            await StandardError.WriteLineAsync("Not a file");
            return 1;
        }

        await using var stream = file.Open(UserContext);
        await stream.CopyToAsync(StandardOutput.BaseStream, cancellationToken);
        return 0;
    }

}
