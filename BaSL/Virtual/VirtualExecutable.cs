using System;
using System.IO;
using System.Threading.Tasks;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualExecutable : VirtualFile, IExecutable
{

    private readonly Func<FileSystem, Stream, Stream, Stream, ReadOnlyMemory<ReadOnlyMemory<char>>, Task<int>> _execute;

    public VirtualExecutable(Path fullPath, Mode mode, Func<FileSystem, Stream, Stream, Stream, ReadOnlyMemory<ReadOnlyMemory<char>>, Task<int>> execute) : base(fullPath, mode) => _execute = execute;

    public Task<int> ExecuteAsync(FileSystem fileSystem, Stream standardInput, Stream standardOutput, Stream standardError, ReadOnlyMemory<ReadOnlyMemory<char>> args)
        => _execute(fileSystem, standardInput, standardOutput, standardError, args);

}
