using System;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public sealed class Process : IDisposable
{

    internal static Process Start(ExecutableContext context, Executable executable, CancellationToken cancellationToken)
    {
        var program = executable(context);
        var task = program.ExecuteAsync(cancellationToken);
        return new Process(context, program, task);
    }

    private readonly ExecutableContext _context;
    private readonly Program _executable;
    private readonly Task<int> _task;

    private Process(ExecutableContext context, Program executable, Task<int> task)
    {
        _context = context;
        _executable = executable;
        _task = task;
    }

    public int ExitCode => !_task.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _task.IsFaulted
            ? -1
            : _task.Result;

    public Task<int> WaitForExitAsync() => _task;

    public void Dispose()
    {
        _task.Dispose();
    }

}
