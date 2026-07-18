using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL;

public sealed class Console
{

    public FileSystem FileSystem { get; }

    public required StreamReader StandardInput { get; init; }

    public required StreamWriter StandardOutput { get; init; }

    public required StreamWriter StandardError { get; init; }

    public Directory CurrentDirectory { get; internal set; }

    public Console(FileSystem fileSystem)
    {
        FileSystem = fileSystem;
        CurrentDirectory = fileSystem.Root;
    }

    public async Task<int> StartAsync()
    {
        var line = await StandardInput.ReadLineAsync();
        var args = line.Split();
        var context = ExecutableContext.Direct(this, FileSystem, args.AsMemory()[1..], StandardInput, StandardOutput, StandardError);
        var program = CurrentDirectory.GetFile(args[0]);
        var process = program.Execute(context, CancellationToken.None);
        await process.WaitForExitAsync();
        return 0;
    }

}
