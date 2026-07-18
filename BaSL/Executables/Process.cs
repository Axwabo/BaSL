using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public sealed class Process
{

    private readonly App _app;
    private readonly Task<int> _task;

    public StreamWriter StandardInput { get; }
    public StreamReader StandardOutput { get; }
    public StreamReader StandardError { get; }

    internal Process(ExecutableContext context, Executable executable, CancellationToken cancellationToken)
    {
        StandardInput = new StreamWriter(context.StandardInput.Writer.AsStream()) {AutoFlush = true};
        StandardOutput = new StreamReader(context.StandardOutput.Reader.AsStream());
        StandardError = new StreamReader(context.StandardError.Reader.AsStream());
        _app = executable(context);
        _task = ExecuteAsync(cancellationToken);
    }

    private async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _app.ExecuteAsync(cancellationToken);
        }
        finally
        {
            await StandardInput.DisposeAsync();
            StandardOutput.Dispose();
            StandardError.Dispose();
            _app.StandardInput.Dispose();
            await _app.StandardOutput.DisposeAsync();
            await _app.StandardError.DisposeAsync();
        }
    }

    public int ExitCode => !_task.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _task.IsFaulted
            ? -1
            : _task.Result;

    public Task<int> WaitForExitAsync() => _task;

}
