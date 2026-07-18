using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class Process
{

    internal static Process Start(Executable executable, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, CancellationToken cancellationToken)
        => new(executable, console, fileSystem, args, cancellationToken);

    private readonly Task<int> _task;

    private readonly StreamReader _standardInput;
    private readonly StreamWriter _standardOutput;
    private readonly StreamWriter _standardError;
    public StreamWriter StandardInput { get; }
    public StreamReader StandardOutput { get; }
    public StreamReader StandardError { get; }

    private Process(Executable executable, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, CancellationToken cancellationToken)
    {
        var stdin = new Pipe();
        var stdout = new Pipe();
        var stderr = new Pipe();
        (_standardInput, StandardInput) = stdin.CreateStreams();
        (StandardOutput, _standardOutput) = stdout.CreateStreams();
        (StandardError, _standardError) = stderr.CreateStreams();
        _task = ExecuteAsync(executable, new ExecutableContext(console, fileSystem, _standardInput, _standardOutput, _standardError, args), cancellationToken);
    }

    private async Task<int> ExecuteAsync(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
    {
        try
        {
            var app = executable(context);
            return await app.ExecuteAsync(cancellationToken);
        }
        finally
        {
            await StandardInput.DisposeAsync();
            StandardOutput.Dispose();
            StandardError.Dispose();
            _standardInput.Dispose();
            await _standardOutput.DisposeAsync();
            await _standardError.DisposeAsync();
        }
    }

    public int ExitCode => !_task.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _task.IsFaulted
            ? -1
            : _task.Result;

    public Task<int> WaitForExitAsync() => _task;

}

file static class Extensions
{

    extension(Pipe pipe)
    {

        public (StreamReader, StreamWriter) CreateStreams() => (
            new StreamReader(pipe.Reader.AsStream()),
            new StreamWriter(pipe.Writer.AsStream()) {AutoFlush = true}
        );

    }

}
