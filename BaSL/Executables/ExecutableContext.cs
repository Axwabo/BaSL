using System;
using System.IO;
using BaSL.FileSystems;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal StreamReader StandardInput { get; }
    internal StreamWriter StandardOutput { get; }
    internal StreamWriter StandardError { get; }
    internal ReadOnlyMemory<string> Args { get; }

    internal ExecutableContext(Console console, FileSystem fileSystem, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError, ReadOnlyMemory<string> args)
    {
        Console = console;
        FileSystem = fileSystem;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
        Args = args;
    }

}
