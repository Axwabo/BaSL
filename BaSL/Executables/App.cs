using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Executables;

public abstract class App
{

    private readonly ExecutableContext _context;

    protected Console Console => _context.Console;

    protected FileSystem FileSystem => _context.FileSystem;

    protected internal StreamReader StandardInput { get; }

    protected internal StreamWriter StandardOutput { get; }

    protected internal StreamWriter StandardError { get; }

    protected ReadOnlyMemory<string> Args => _context.Args;

    protected App(ExecutableContext context)
    {
        _context = context;
        StandardInput = new StreamReader(context.StandardInput.Reader.AsStream());
        StandardOutput = new StreamWriter(context.StandardOutput.Writer.AsStream()) {AutoFlush = true};
        StandardError = new StreamWriter(context.StandardError.Writer.AsStream()) {AutoFlush = true};
    }

    public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);

}
