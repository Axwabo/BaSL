using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.Shell.Console;

public sealed class Bytes : App
{

    public Bytes(ExecutableContext context) : base(context)
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
        var buffer = new byte[32];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        foreach (var b in buffer.AsSpan(0, read))
            StandardOutput.WriteLine(b);
        return 0;
    }

}
