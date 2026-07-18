using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using Directory = BaSL.FileSystems.Directory;
using Path = BaSL.FileSystems.Path;

namespace BaSL.Shell.Console;

public sealed class Cd : Executables.Program
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
