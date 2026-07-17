using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Executables;

public abstract class Program
{

    private readonly ExecutableContext _context;

    protected Console Console => _context.Console;

    protected FileSystem FileSystem => _context.FileSystem;

    protected PipeReader StandardInput => _context.StandardInput;

    protected PipeWriter StandardOutput => _context.StandardOutput;

    protected PipeWriter StandardError => _context.StandardError;

    protected ReadOnlyMemory<string> Args => _context.Args;

    protected Program(ExecutableContext context) => _context = context;

    public abstract Task<int> ExecuteAsync();

}
