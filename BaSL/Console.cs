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

    private CancellationTokenSource? _cts;

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
        var binaries = CurrentDirectory;
        while (true)
        {
            await StandardOutput.WriteAsync('#');
            var line = await StandardInput.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
                continue;
            if (line.AsSpan().Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                return 0;
            var cts = _cts = new CancellationTokenSource();
            var token = cts.Token;
            try
            {
                var args = line.Split();
                var context = new RootExecutableContext(ExecutableContext.Piped(this, FileSystem, args.AsMemory()[1..]), StandardInput, StandardOutput, StandardError);
                var program = binaries.GetFile(args[0]);
                var process = program.Execute(context, token);
                await process.WaitForExitAsync();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            finally
            {
                cts.Dispose();
                _cts = null;
            }
        }
    }

    public void TerminateCurrentProcess() => _cts?.Cancel();

}
