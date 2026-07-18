using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Executables;

public abstract class Program
{

    private readonly ExecutableContext _context;

    protected Console Console => _context.Console;

    protected FileSystem FileSystem => _context.FileSystem;

    protected StreamReader StandardInput { get; }

    protected StreamWriter StandardOutput { get; }

    protected StreamWriter StandardError { get; }

    protected ReadOnlyMemory<string> Args => _context.Args;

    protected Program(ExecutableContext context)
    {
        _context = context;
        StandardInput = new StreamReader(context.StandardInput.Reader.AsStream());
        StandardOutput = new StreamWriter(context.StandardOutput.Writer.AsStream());
        StandardError = new StreamWriter(context.StandardError.Writer.AsStream());
    }

    public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);

}
