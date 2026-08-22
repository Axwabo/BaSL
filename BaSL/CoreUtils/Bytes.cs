using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

[Help("""
      Writes the first 32 bytes of a file to stdout.
      If less than 32 bytes are available, prints the read bytes.
      """)]
public sealed partial class Bytes : App
{

    public Bytes(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.IsEmpty)
            return await ErrorAsync("Argument required", cancellationToken);
        var entry = FileSystem.ResolveFile(Args[0]);
        if (!entry.Success)
            return await ErrorAsync(entry.Error, cancellationToken);
        var open = entry.Value.Open(UserContext, OpenMode.Read);
        if (!open.Success)
            return await ErrorAsync(open.Error, cancellationToken);
        await using var stream = open.Value;
        var buffer = new byte[32];
        var read = await stream.ReadAsync(buffer, cancellationToken);
        foreach (var b in buffer.AsSpan(0, read))
            StandardOutput.WriteLine(b);
        return 0;
    }

}
