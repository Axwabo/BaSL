using System;
using System.IO.Pipelines;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Pipe StandardInput { get; }
    internal Pipe StandardOutput { get; }
    internal Pipe StandardError { get; }
    internal ReadOnlyMemory<string> Args { get; }

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
