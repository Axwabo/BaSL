using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public sealed class Process
{

    internal static Process Start(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
        => new(executable, context, cancellationToken);

    private readonly Task<int> _exit;

    private Process(Executable executable, ExecutableContext context, CancellationToken cancellationToken)
    {
        StandardInput = context.DestinationInput;
        StandardOutput = context.DestinationOutput;
        StandardError = context.DestinationError;
        _exit = executable(context).ExecuteAsync(cancellationToken);
    }

    public StreamWriter StandardInput { get; }
    public StreamReader StandardOutput { get; }
    public StreamReader StandardError { get; }

    public int ExitCode => !_exit.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _exit.IsFaulted
            ? -1
            : _exit.Result;

    public Task<int> WaitForExitAsync() => _exit;

}
