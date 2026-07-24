using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Cat : App
{

    public Cat(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var entry = FileSystem.ResolveFile(arg);
            if (!entry.Success)
            {
                await StandardError.WriteLineAsync(entry.Error.Message, cancellationToken);
                return 1;
            }

            var open = entry.Value.Open(UserContext, OpenMode.Read);
            if (!open.Success)
            {
                await StandardError.WriteLineAsync(open.Error.Message, cancellationToken);
                return 1;
            }

            await using var stream = open.Value;
            await stream.CopyToAsync(StandardOutput.BaseStream, cancellationToken);
        }

        return 0;
    }

}
