using System.IO;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    public FileSystem FileSystem { get; }
    public Stream StandardInput { get; }
    public Stream StandardOutput { get; }
    public Stream StandardError { get; }
    public string[] Args { get; }

    internal ExecutableContext(FileSystem fileSystem, Stream standardInput, Stream standardOutput, Stream standardError, string[] args)
    {
        FileSystem = fileSystem;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Args = args;
    }

}
