using System;
using System.IO.Pipelines;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    public Console Console { get; }
    public FileSystem FileSystem { get; }
    public Pipe StandardInput { get; }
    public Pipe StandardOutput { get; }
    public Pipe StandardError { get; }
    public ReadOnlyMemory<string> Args { get; }

    internal ExecutableContext(Console console, FileSystem fileSystem, Pipe standardInput, Pipe standardOutput, Pipe standardError, ReadOnlyMemory<string> args)
    {
        Console = console;
        FileSystem = fileSystem;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Args = args;
    }

}
