using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    internal static ExecutableContext Root(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError)
    {
        var inPipe = new PipeWrapper();
        var outPipe = new PipeWrapper();
        var errPipe = new PipeWrapper();
        return new ExecutableContext(
            console,
            fileSystem,
            console.CurrentDirectory,
            args,
            standardInput,
            standardOutput,
            standardError,
            inPipe.Writer,
            outPipe.Reader,
            errPipe.Reader
        )
        {
            Final = true,
            StandardInput = inPipe,
            StandardOutput = outPipe,
            StandardError = errPipe
        };
    }

    internal static ExecutableContext Piped(Console console, FileSystem fileSystem)

    private ExecutableContext(
        Console console,
        FileSystem fileSystem,
        Directory workingDirectory,
        ReadOnlyMemory<string> args,
        StreamReader sourceInput,
        StreamWriter sourceOutput,
        StreamWriter sourceError,
        StreamWriter destinationInput,
        StreamReader destinationOutput,
        StreamReader destinationError
    )
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        SourceInput = sourceInput;
        SourceOutput = sourceOutput;
        SourceError = sourceError;
        DestinationInput = destinationInput;
        DestinationError = destinationError;
        DestinationOutput = destinationOutput;
    }

    private bool Final { get; set; }
    private PipeWrapper? StandardInput { get; init; }
    private PipeWrapper? StandardOutput { get; init; }
    private PipeWrapper? StandardError { get; init; }
    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader SourceInput { get; private set; }
    internal StreamWriter SourceOutput { get; private set; }
    internal StreamWriter SourceError { get; private set; }
    internal StreamWriter DestinationInput { get; }
    internal StreamReader DestinationOutput { get; }
    internal StreamReader DestinationError { get; }

    private void Connect(ExecutableContext source)
    {
        if (Final)
            return;
        Final = true;
        if (source.StandardInput != null)
            SourceInput = source.StandardInput.Reader;
        if (source.StandardOutput != null)
            SourceOutput = source.StandardOutput.Writer;
        if (source.StandardError != null)
            SourceError = source.StandardError.Writer;
    }

    internal async Task CopyAsync()
    {
    }

}
