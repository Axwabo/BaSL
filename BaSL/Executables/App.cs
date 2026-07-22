using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public abstract class App
{

    private readonly ExecutableContext _context;

    protected App(ExecutableContext context) => _context = context;

    protected Console Console => _context.Console;

    protected UserContext UserContext => Console.UserContext;

    protected FileSystem FileSystem => _context.FileSystem;

    protected Directory WorkingDirectory => _context.WorkingDirectory;

    protected internal StreamReader StandardInput => _context.ConsumerInput;

    protected internal StreamWriter StandardOutput => _context.ConsumerOutput;

    protected internal StreamWriter StandardError => _context.ConsumerError;

    protected ReadOnlyMemory<string> Args => _context.Args;

    public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);

}
