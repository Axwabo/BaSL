using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

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

    internal static PipedExecutableContext Piped(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        var (inRead, inWrite) = CreateStreams();
        var (outRead, outWrite) = CreateStreams();
        var (errRead, errWrite) = CreateStreams();
        return new PipedExecutableContext(
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

    private protected ExecutableContext(Console console, FileSystem fileSystem, Directory workingDirectory, ReadOnlyMemory<string> args, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError)
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        StandardInput = standardInput;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader StandardInput { get; }
    internal StreamWriter StandardOutput { get; }
    internal StreamWriter StandardError { get; }

}

internal sealed class PipedExecutableContext : ExecutableContext, IAsyncDisposable
{

    public PipedExecutableContext(
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
    ) : base(console, fileSystem, workingDirectory, args, standardInputReader, standardOutputWriter, standardErrorWriter)
    {
        StandardInputWriter = standardInputWriter;
        StandardOutputReader = standardOutputReader;
        StandardErrorReader = standardErrorReader;
    }

    public StreamWriter StandardInputWriter { get; }
    public StreamReader StandardOutputReader { get; }
    public StreamReader StandardErrorReader { get; }

    public async ValueTask DisposeAsync()
    {
        StandardInput.Dispose();
        await StandardOutput.DisposeAsync();
        await StandardError.DisposeAsync();
        await StandardInputWriter.DisposeAsync();
        StandardOutputReader.Dispose();
        StandardErrorReader.Dispose();
    }

}
