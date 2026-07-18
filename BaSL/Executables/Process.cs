using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public sealed class Process
{

    internal static Process Start(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
        => new(executable, context, cancellationToken);

    private readonly Task<int> _task;

    private readonly StreamReader _standardInput;
    private readonly StreamWriter _standardOutput;
    private readonly StreamWriter _standardError;
    public StreamWriter StandardInput { get; }
    public StreamReader StandardOutput { get; }
    public StreamReader StandardError { get; }

    private Process(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
    {
        _standardInput = context.StandardInput;
        _standardOutput = context.StandardOutput;
        _standardError = context.StandardError;
        if (context is PipedExecutableContext piped)
        {
            StandardInput = piped.StandardInputWriter;
            StandardOutput = piped.StandardOutputReader;
            StandardError = piped.StandardErrorReader;
        }
        else
        {
            StandardInput = null!;
            StandardOutput = null!;
            StandardError = null!;
        }

        _task = ExecuteAsync(executable, context, cancellationToken);
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
            _standardInput.Dispose();
            await _standardOutput.DisposeAsync();
            await _standardError.DisposeAsync();
            if (context is PipedExecutableContext piped)
                await piped.DisposeAsync();
        }
    }

    public int ExitCode => !_task.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _task.IsFaulted
            ? -1
            : _task.Result;

    public Task<int> WaitForExitAsync() => _task;

}
