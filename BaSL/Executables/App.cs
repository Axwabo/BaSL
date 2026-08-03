using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;
using BaSL.Syntax;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public abstract class App
{

    private protected readonly ExecutableContext Context;

    protected App(ExecutableContext context) => Context = context;

    protected Console Console => Context.Console;

    protected UserContext UserContext => Console.UserContext;

    protected FileSystem FileSystem => Context.FileSystem;

    protected Directory WorkingDirectory => Context.WorkingDirectory;

    protected internal StreamReader StandardInput => Context.SourceInput;

    protected internal StreamWriter StandardOutput => Context.SourceOutput;

    protected internal StreamWriter StandardError => Context.SourceError;

    protected Args Args => Context.Args;

    public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);

    protected async Task<int> ShellExecuteAsync(ShellStatement statement, CancellationToken cancellationToken)
    {
        var shell = new BaShell(Context, statement);
        return await shell.ExecuteAsync(cancellationToken);
    }

}
