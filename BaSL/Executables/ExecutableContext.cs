using System.IO;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    public FileSystem FileSystem { get; }
    public StreamReader StandardInput { get; }
    public StreamWriter StandardOutput { get; }
    public StreamWriter StandardError { get; }
    public string[] Args { get; }

    internal ExecutableContext(FileSystem fileSystem, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError, string[] args)
    {
        FileSystem = fileSystem;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Args = args;
    }

}
