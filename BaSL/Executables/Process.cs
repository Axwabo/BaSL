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
            copyIn = context.ConsumerInput.BaseStream.CopyToAsync(context.ProducerInput.BaseStream, cancellationToken);
            copyOut = context.ProducerOutput.BaseStream.CopyToAsync(context.ConsumerOutput.BaseStream, cancellationToken);
            copyErr = context.ProducerError.BaseStream.CopyToAsync(context.ConsumerError.BaseStream, cancellationToken);
            return await app.ExecuteAsync(cancellationToken);
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
        StandardInput = context.ProducerInput;
        StandardOutput = context.ProducerOutput;
        StandardError = context.ProducerError;
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
