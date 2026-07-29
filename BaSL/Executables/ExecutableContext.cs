using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    internal static ExecutableContext Root(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamWriter standardOutput, StreamWriter standardError)
        => new(console, fileSystem, console.CurrentDirectory, args)
        {
            SourceOutput = standardOutput,
            SourceError = standardError
        };

    internal static ExecutableContext Piped(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args) => new(console, fileSystem, console.CurrentDirectory, args)
    {
        Parent = source
    };

    internal static ExecutableContext Sunken(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamWriter standardOutput, StreamWriter standardError)
        => new(console, fileSystem, console.CurrentDirectory, args)
        {
            SourceOutput = standardOutput,
            SourceError = standardError,
            DisposeOutput = true
        };

    private static async Task CopyAsync(StreamReader source, StreamWriter destination, PipeWrapper cancellation, bool dispose = false)
    {
        try
        {
            await source.BaseStream.CopyToAsync(destination.BaseStream, cancellation.CancellationToken);
        }
        catch (InvalidOperationException)
        {
            // "Reading is not allowed after reader was completed." NERD EMOJI
        }
        finally
        {
            if (dispose)
                await destination.DisposeAsync();
        }
    }

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

    private ExecutableContext? Parent { get; init; }
    internal PipeWrapper? StandardInput { get; init; }
    internal PipeWrapper? StandardOutput { get; init; }
    internal PipeWrapper? StandardError { get; init; }
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
    private bool DisposeOutput { get; init; }
    internal bool IsRoot => Parent == null;

    internal async Task CopyAsync(bool copyStdin)
    {
        if (Parent == null)
            return;
        try
        {
            await Task.WhenAll(
                copyStdin ? CopyAsync(Parent.SourceInput, DestinationInput, StandardInput) : Task.CompletedTask,
                CopyAsync(DestinationOutput, Parent.SourceOutput, StandardOutput, DisposeOutput),
                CopyAsync(DestinationError, Parent.SourceError, StandardError)
            );
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
        if (DisposeOutput)
            await SourceOutput.DisposeAsync();
    }

}
