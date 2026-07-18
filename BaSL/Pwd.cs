using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL;

public sealed class Pwd : Program
{

    public Pwd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var writer = StandardOutput;
        await writer.WriteLineAsync(Console.CurrentDirectory.FullPath.Value);
        return 0;
    }

}
