using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL;

public sealed class Cd : Program
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    public override Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            Console.CurrentDirectory = FileSystem.Home;
        else
            Console.CurrentDirectory = (Directory) FileSystem.Resolve(Path.Combine(Console.CurrentDirectory.FullPath, Args.Span[0]));
        return Task.FromResult(0);
    }

}
