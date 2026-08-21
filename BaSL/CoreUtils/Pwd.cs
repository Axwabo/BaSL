using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.CoreUtils;

[Help("Prints the current directory to stdout.")]
public sealed partial class Pwd : App
{

    public Pwd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        await StandardOutput.WriteLineAsync(WorkingDirectory.FullPath, cancellationToken);
        return 0;
    }

}
