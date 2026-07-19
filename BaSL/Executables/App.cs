using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public abstract class App
{

    private readonly ExecutableContext _context;

    protected Console Console => _context.Console;

    protected FileSystem FileSystem => _context.FileSystem;

    protected Directory WorkingDirectory => _context.WorkingDirectory;

    protected internal StreamReader StandardInput => _context.StandardInput;

    protected internal StreamWriter StandardOutput => _context.StandardOutput;

    protected internal StreamWriter StandardError => _context.StandardError;

    protected ReadOnlyMemory<string> Args => _context.Args;

    protected App(ExecutableContext context) => _context = context;

    public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);

}
