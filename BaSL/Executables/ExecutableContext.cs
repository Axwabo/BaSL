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
        => new(console, fileSystem, console.CurrentDirectory, args)
        {
            SourceInput = standardInput,
            SourceOutput = standardOutput,
            SourceError = standardError
        };

    internal static ExecutableContext Piped(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args) => new(console, fileSystem, console.CurrentDirectory, args)
    {
        SourceInput = source.StandardInput.Reader,
        SourceOutput = source.StandardOutput.Writer,
        SourceError = source.StandardError.Writer
    };

    private bool _disposed;

    private ExecutableContext(
        Console console,
        FileSystem fileSystem,
        Directory workingDirectory,
        ReadOnlyMemory<string> args
    )
    {
        StandardInput = new PipeWrapper();
        StandardOutput = new PipeWrapper();
        StandardError = new PipeWrapper();
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        SourceInput = StandardInput.Reader;
        SourceOutput = StandardOutput.Writer;
        SourceError = StandardError.Writer;
        DestinationInput = StandardInput.Writer;
        DestinationOutput = StandardOutput.Reader;
        DestinationError = StandardError.Reader;
    }

    internal PipeWrapper StandardInput { get; }
    internal PipeWrapper StandardOutput { get; }
    internal PipeWrapper StandardError { get; }
    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader SourceInput { get; private init; }
    internal StreamWriter SourceOutput { get; private init; }
    internal StreamWriter SourceError { get; private init; }
    internal StreamWriter DestinationInput { get; }
    internal StreamReader DestinationOutput { get; }
    internal StreamReader DestinationError { get; }

    internal async Task CopyAsync()
    {
        try
        {
            await /*Task.WhenAll(
                SourceInput.BaseStream is ReaderStream ? SourceInput.BaseStream.CopyToAsync(DestinationInput.BaseStream, StandardInput.CancellationToken) : Task.CompletedTask,*/
                DestinationOutput.BaseStream.CopyToAsync(SourceOutput.BaseStream, StandardOutput.CancellationToken) /*,
                DestinationError.BaseStream.CopyToAsync(SourceError.BaseStream, StandardError.CancellationToken)
            )*/;
        }
        catch (OperationCanceledException) when (_disposed)
        {
        }
    }

    internal async ValueTask DisposeAsync()
    {
        _disposed = true;
        await StandardInput.DisposeAsync();
        await StandardOutput.DisposeAsync();
        await StandardError.DisposeAsync();
    }

}
