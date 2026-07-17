using System;
using System.IO.Pipelines;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    public Console Console { get; }
    public FileSystem FileSystem { get; }
    public PipeReader StandardInput { get; }
    public PipeWriter StandardOutput { get; }
    public PipeWriter StandardError { get; }
    public ReadOnlyMemory<string> Args { get; }

    internal ExecutableContext(Console console, FileSystem fileSystem, PipeReader standardInput, PipeWriter standardOutput, PipeWriter standardError, ReadOnlyMemory<string> args)
    {
        Console = console;
        FileSystem = fileSystem;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Args = args;
    }

}
