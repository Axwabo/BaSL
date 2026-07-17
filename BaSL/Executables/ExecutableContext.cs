using System.IO.Pipelines;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    public FileSystem FileSystem { get; }
    public PipeReader StandardInput { get; }
    public PipeWriter StandardOutput { get; }
    public PipeWriter StandardError { get; }
    public string[] Args { get; }

    internal ExecutableContext(FileSystem fileSystem, PipeReader standardInput, PipeWriter standardOutput, PipeWriter standardError, string[] args)
    {
        FileSystem = fileSystem;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Args = args;
    }

}
