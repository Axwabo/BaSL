using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Mkdir : App
{

    public Mkdir(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
        {
            await StandardError.WriteLineAsync("Missing operand", cancellationToken);
            return 1;
        }

        var result = WorkingDirectory.CreateDirectories(Args.Span[0]);
        if (result.Success)
            return 0;
        await StandardError.WriteLineAsync(result.Error.Message, cancellationToken);
        return 1;
    }

}
