using System;
using System.IO;
using System.IO.Pipelines;
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

    public async Task<int> ExecuteAsync()
    {
        var line = await StandardInput.ReadLineAsync();
        var args = line.Split();
        var program = CurrentDirectory.GetFile(args[0]);
        var stdin = new Pipe();
        var stdout = new Pipe();
        var stderr = new Pipe();
        var context = new ExecutableContext(this, FileSystem, stdin, stdout, stderr, args.AsMemory()[1..]);
        var process = program.Execute(context, CancellationToken.None);
        var copyStdout = stdout.Reader.CopyToAsync(StandardOutput.BaseStream);
        var copyStdin = stderr.Reader.CopyToAsync(StandardError.BaseStream);
        await process.WaitForExitAsync();
        await Task.WhenAll(
            stdout.Writer.CompleteAsync().AsTask(),
            stderr.Writer.CompleteAsync().AsTask(),
            copyStdin,
            copyStdout
        );
        return 0;
    }

}
