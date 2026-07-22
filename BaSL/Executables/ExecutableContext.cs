using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public class ExecutableContext
{

    private static (StreamReader, StreamWriter) CreateStreams()
    {
        var pipe = new PipeWrapper();
        return (
            new StreamReader(pipe.Reader),
            new StreamWriter(pipe.Writer) {AutoFlush = true}
        );
    }

    internal static ExecutableContext Piped(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        var (inRead, inWrite) = CreateStreams();
        var (outRead, outWrite) = CreateStreams();
        var (errRead, errWrite) = CreateStreams();
        return new ExecutableContext(
            console,
            fileSystem,
            console.CurrentDirectory,
            args,
            inRead,
            inWrite,
            outRead,
            outWrite,
            errRead,
            errWrite
        );
    }

    private protected ExecutableContext(
        Console console,
        FileSystem fileSystem,
        Directory workingDirectory,
        ReadOnlyMemory<string> args,
        StreamReader standardInputReader,
        StreamWriter standardInputWriter,
        StreamReader standardOutputReader,
        StreamWriter standardOutputWriter,
        StreamReader standardErrorReader,
        StreamWriter standardErrorWriter
    )
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        StandardInput = standardInputReader;
        StandardOutput = standardOutputWriter;
        StandardError = standardErrorWriter;
        StandardInputWriter = standardInputWriter;
        StandardErrorReader = standardErrorReader;
        StandardOutputReader = standardOutputReader;
    }

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader StandardInput { get; }
    internal StreamWriter StandardOutput { get; }
    internal StreamWriter StandardError { get; }
    internal StreamWriter StandardInputWriter { get; }
    internal StreamReader StandardOutputReader { get; }
    internal StreamReader StandardErrorReader { get; }

    internal async ValueTask DisposeAsync()
    {
        StandardInput.Dispose();
        await StandardOutput.DisposeAsync();
        await StandardError.DisposeAsync();
        await StandardInputWriter.DisposeAsync();
        StandardOutputReader.Dispose();
        StandardErrorReader.Dispose();
    }

}

internal sealed class RootExecutableContext : ExecutableContext
{

    internal RootExecutableContext(ExecutableContext other, StreamReader input, StreamWriter output, StreamWriter error) : base(
        other.Console,
        other.FileSystem,
        other.WorkingDirectory,
        other.Args,
        other.StandardInput,
        other.StandardInputWriter,
        other.StandardOutputReader,
        other.StandardOutput,
        other.StandardErrorReader,
        other.StandardError
    )
    {
        Input = input;
        Output = output;
        Error = error;
    }

    public StreamReader Input { get; }
    public StreamWriter Output { get; }
    public StreamWriter Error { get; }

}
