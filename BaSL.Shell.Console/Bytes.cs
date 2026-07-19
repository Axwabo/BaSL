using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using File = BaSL.FileSystems.File;

namespace BaSL.Shell.Console;

public sealed class Bytes : App
{

    public Bytes(ExecutableContext context) : base(context)
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

        await using var stream = file.Open();
        var buffer = new byte[32];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        foreach (var b in buffer.AsSpan(0, read))
            StandardOutput.WriteLine(b);
        return 0;
    }

}
