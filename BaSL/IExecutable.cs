using System;
using System.IO;
using System.Threading.Tasks;

namespace BaSL.FileSystems;

public interface IExecutable
{

    Task<int> ExecuteAsync(FileSystem fileSystem, Stream standardInput, Stream standardOutput, Stream standardError, ReadOnlyMemory<ReadOnlyMemory<char>> args);

}
