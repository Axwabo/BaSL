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

    public Console(FileSystem fileSystem)
    {
        FileSystem = fileSystem;
        CurrentDirectory = fileSystem.Root;
    }

    public FileSystem FileSystem { get; }

    public required StreamReader StandardInput { get; init; }

    public required StreamWriter StandardOutput { get; init; }

    public required StreamWriter StandardError { get; init; }

    public Directory CurrentDirectory { get; internal set; }

    public async Task<int> StartAsync()
    {
        while (true)
        {
            await StandardOutput.WriteAsync('#');
            var line = await StandardInput.ReadLineAsync();
            if (line.AsSpan().Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                return 0;
            var args = line.Split();
            var context = ExecutableContext.Piped(this, FileSystem, args.AsMemory()[1..]);
            var program = CurrentDirectory.GetFile(args[0]);
            var process = program.Execute(context, CancellationToken.None);
            await Task.WhenAll(
                process.WaitForExitAsync(),
                context.StandardOutputReader.BaseStream.CopyToAsync(StandardOutput.BaseStream),
                context.StandardErrorReader.BaseStream.CopyToAsync(StandardError.BaseStream)
            );
        }
    }

}
