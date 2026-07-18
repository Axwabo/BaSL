using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using BaSL.FileSystems;

namespace BaSL.Executables;

public class ExecutableContext
{

    private static (StreamReader, StreamWriter) CreateStreams()
    {
        var pipe = new Pipe();
        return (
            new StreamReader(pipe.Reader.AsStream()),
            new StreamWriter(pipe.Writer.AsStream()) {AutoFlush = true}
        );
    }

    internal static ExecutableContext Direct(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError)
        => new(console, fileSystem, args, standardInput, standardOutput, standardError);

    internal static PipedExecutableContext Piped(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        var (inRead, inWrite) = CreateStreams();
        var (outRead, outWrite) = CreateStreams();
        var (errRead, errWrite) = CreateStreams();
        return new PipedExecutableContext(
            console,
            fileSystem,
            args,
            inRead,
            inWrite,
            outRead,
            outWrite,
            errRead,
            errWrite
        );
    }

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader StandardInput { get; }
    internal StreamWriter StandardOutput { get; }
    internal StreamWriter StandardError { get; }

    private protected ExecutableContext(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError)
    {
        Console = console;
        FileSystem = fileSystem;
        Args = args;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

}

internal sealed class PipedExecutableContext : ExecutableContext, IAsyncDisposable
{

    public StreamWriter StandardInputWriter { get; }
    public StreamReader StandardOutputReader { get; }
    public StreamReader StandardErrorReader { get; }

    public PipedExecutableContext(
        Console console,
        FileSystem fileSystem,
        ReadOnlyMemory<string> args,
        StreamReader standardInputReader,
        StreamWriter standardInputWriter,
        StreamReader standardOutputReader,
        StreamWriter standardOutputWriter,
        StreamReader standardErrorReader,
        StreamWriter standardErrorWriter
    ) : base(console, fileSystem, args, standardInputReader, standardOutputWriter, standardErrorWriter)
    {
        StandardInputWriter = standardInputWriter;
        StandardOutputReader = standardOutputReader;
        StandardErrorReader = standardErrorReader;
    }

    public async ValueTask DisposeAsync()
    {
        await StandardInputWriter.DisposeAsync();
        StandardOutputReader.Dispose();
        StandardErrorReader.Dispose();
    }

}
