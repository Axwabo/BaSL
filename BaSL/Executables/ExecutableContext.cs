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
        Parent = source
    };

    private Task? _copyErr;

    private Task? _copyIn;
    private Task? _copyOut;

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
    internal PipeWrapper StandardInput { get; }
    internal PipeWrapper StandardOutput { get; }
    internal PipeWrapper StandardError { get; }
    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }

    internal StreamReader SourceInput
    {
        get
        {
            _copyIn ??= CopyAsync(Parent?.SourceInput, DestinationInput, StandardInput);
            return field;
        }
        private init;
    }

    internal StreamWriter SourceOutput
    {
        get
        {
            _copyOut ??= CopyAsync(DestinationOutput, Parent?.SourceOutput, StandardOutput);
            return field;
        }
        private init;
    }

    internal StreamWriter SourceError
    {
        get
        {
            _copyErr ??= CopyAsync(DestinationError, Parent?.SourceError, StandardError);
            return field;
        }
        private init;
    }

    internal StreamWriter DestinationInput { get; }
    internal StreamReader DestinationOutput { get; }
    internal StreamReader DestinationError { get; }

    private async Task CopyAsync(StreamReader? source, StreamWriter? destination, PipeWrapper cancellation)
    {
        if (source == null || destination == null)
            return;
        try
        {
            await source.BaseStream.CopyToAsync(destination.BaseStream, cancellation.CancellationToken);
        }
        catch (OperationCanceledException) when (_disposed)
        {
        }
        catch (InvalidOperationException)
        {
            // "Reading is not allowed after reader was completed." NERD EMOJI
        }
    }

    internal async ValueTask DisposeAsync()
    {
        _disposed = true;
        await StandardInput.DisposeAsync();
        await StandardOutput.DisposeAsync();
        await StandardError.DisposeAsync();
        await Task.WhenAll(
            _copyIn ?? Task.CompletedTask,
            _copyOut ?? Task.CompletedTask,
            _copyErr ?? Task.CompletedTask
        );
    }

}
