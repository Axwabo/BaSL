using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public sealed class Process
{

    internal static Process Start(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
        => new(executable, context, cancellationToken);

    private static async Task<int> ExecuteAsync(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
    {
        var copyIn = Task.CompletedTask;
        var copyOut = Task.CompletedTask;
        var copyErr = Task.CompletedTask;
        try
        {
            var app = executable(context);
            var execute = app.ExecuteAsync(cancellationToken);
            if (context is not RootExecutableContext root)
                return await execute;
            copyIn = context.StandardInput.BaseStream.CopyToAsync(root.Input.BaseStream, cancellationToken);
            copyOut = context.StandardOutputReader.BaseStream.CopyToAsync(root.Output.BaseStream, cancellationToken);
            copyErr = context.StandardErrorReader.BaseStream.CopyToAsync(root.Error.BaseStream, cancellationToken);
            return await execute;
        }
        finally
        {
            await context.DisposeAsync();
            await Task.WhenAll(copyIn, copyOut, copyErr);
        }
    }

    private readonly Task<int> _task;

    private Process(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
    {
        StandardInput = context.StandardInputWriter;
        StandardOutput = context.StandardOutputReader;
        StandardError = context.StandardErrorReader;
        _task = ExecuteAsync(executable, context, cancellationToken);
    }

    public StreamWriter StandardInput { get; }
    public StreamReader StandardOutput { get; }
    public StreamReader StandardError { get; }

    public int ExitCode => !_task.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _task.IsFaulted
            ? -1
            : _task.Result;

    public Task<int> WaitForExitAsync() => _task;

}
