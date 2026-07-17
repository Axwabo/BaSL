using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Shell;

public abstract class Program
{

    public required FileSystem FileSystem { get; init; }
    public required Stream StandardInput { get; init; }
    public required Stream StandardOutput { get; init; }
    public required Stream StandardError { get; init; }
    public required ReadOnlyMemory<ReadOnlyMemory<char>> Arguments { get; init; }

    public abstract Task<int> ExecuteAsync();

}
