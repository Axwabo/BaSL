using BaSL.Executables;
using BaSL.FileSystems.Extensions;
using BaSL.FileSystems.Virtual;
using Directory = BaSL.FileSystems.Directory;
using Path = BaSL.FileSystems.Path;

namespace BaSL.Shell.Console;

public sealed class Cd : App
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    public override Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            Console.CurrentDirectory = (FileSystem as VirtualFileSystem)?.Home ?? FileSystem.Root;
        else
            Console.CurrentDirectory = (Directory) FileSystem.Resolve(Path.Combine(Console.CurrentDirectory.FullPath, Args.Span[0]));
        return Task.FromResult(0);
    }

}
