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
        try
        {
            var app = executable(context);
            var execute = app.ExecuteAsync(cancellationToken);
            if (context is not RootExecutableContext root)
                return await execute;
            await Task.WhenAll(
                execute,
                context.StandardOutputReader.BaseStream.CopyToAsync(root.Output.BaseStream, cancellationToken),
                context.StandardErrorReader.BaseStream.CopyToAsync(root.Error.BaseStream, cancellationToken)
            );
            return execute.Result;
        }
        finally
        {
            await context.DisposeAsync();
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
