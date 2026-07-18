using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        var program = CurrentDirectory.GetFile(args[0]);
        var process = program.Execute(this, FileSystem, args.AsMemory()[1..], CancellationToken.None);
        await Task.WhenAll(
            process.WaitForExitAsync(),
            process.StandardError.BaseStream.CopyToAsync(StandardError.BaseStream),
            process.StandardOutput.BaseStream.CopyToAsync(StandardOutput.BaseStream)
        );
        return 0;
    }

}
