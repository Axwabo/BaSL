using System;
using System.Threading.Tasks;

namespace BaSL.Executables;

public sealed class Process
{

    private readonly ExecutableContext _context;
    private readonly Task<int> _task;

    internal Process(ExecutableContext context, Executable executable)
    {
        _context = context;
        _task = executable(context);
    }

    public int ExitCode => !_task.IsCompleted
        ? throw new InvalidOperationException("Process has not exited")
        : _task.IsFaulted
            ? -1
            : _task.Result;

    public Task<int> WaitForExitAsync() => _task;

}
