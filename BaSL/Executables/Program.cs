using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Executables;

public abstract class Program
{

    private readonly ExecutableContext _context;

    protected FileSystem FileSystem => _context.FileSystem;

    protected StreamReader StandardInput => _context.StandardInput;

    protected StreamWriter StandardOutput => _context.StandardOutput;

    protected StreamWriter StandardError => _context.StandardError;

    protected ReadOnlySpan<string> Args => _context.Args;

    protected Program(ExecutableContext context) => _context = context;

    public abstract Task<int> ExecuteAsync();

}
