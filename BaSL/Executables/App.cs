using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public abstract class App
{

    protected App(ExecutableContext context) => Context = context;

    private protected ExecutableContext Context { get; init; }

    protected BaShell Shell => Context.Shell;

    protected UserContext UserContext => Shell.UserContext;

    protected FileSystem FileSystem => Context.FileSystem;

    protected Directory WorkingDirectory => Context.WorkingDirectory;

    protected internal StreamReader StandardInput => Context.SourceInput;

    protected internal StreamWriter StandardOutput => Context.SourceOutput;

    protected internal StreamWriter StandardError => Context.SourceError;

    protected Args Args => Context.Args;

    protected IReadOnlyDictionary<string, string> Environment => Context.Environment;

    public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);

    protected async Task<int> ErrorAsync(Error error, CancellationToken cancellationToken)
    {
        await StandardError.WriteLineAsync(error, cancellationToken);
        return 1;
    }

    protected async Task<int> ErrorAsync(string error, CancellationToken cancellationToken)
    {
        await StandardError.WriteLineAsync(error, cancellationToken);
        return 1;
    }

}
