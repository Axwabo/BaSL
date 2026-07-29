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
        => new ExecutableContext(console, fileSystem, args)
        {
            _sourceOutput = standardOutput,
            _sourceError = standardError
        }.CreateStdinPipe();

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

    private static T ThrowIfNull<T>(T? returnValue) => returnValue ?? throw new InvalidOperationException("Context has not yet been initialized, this should not happen!");
    private StreamReader? _destinationError;
    private StreamWriter? _destinationInput;
    private StreamReader? _destinationOutput;

    private bool _disposed;
    private StreamWriter? _sourceError;
    private StreamReader? _sourceInput;
    private StreamWriter? _sourceOutput;

    private ExecutableContext(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = console.CurrentDirectory;
        Args = args;
    }

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }

    private ExecutableContext? Parent { get; init; }
    internal PipeWrapper? StandardInput { get; private set; }
    internal PipeWrapper? StandardOutput { get; private set; }
    internal PipeWrapper? StandardError { get; private set; }

    internal StreamReader SourceInput => ThrowIfNull(_sourceInput);

    internal StreamWriter SourceOutput => ThrowIfNull(_sourceOutput);

    internal StreamWriter SourceError => ThrowIfNull(_sourceError);

    internal StreamWriter DestinationInput => ThrowIfNull(_destinationInput);

    internal StreamReader DestinationOutput => ThrowIfNull(_destinationOutput);

    internal StreamReader DestinationError => ThrowIfNull(_destinationError);

    private bool DisposeOutput { get; init; }
    internal bool IsRoot => Parent == null;

    private ExecutableContext CreateStdinPipe()
    {
        StandardInput = new PipeWrapper();
        _sourceInput = StandardInput.Reader;
        _destinationInput ??= StandardInput.Writer;
        return this;
    }

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
        if (StandardInput != null)
            await StandardInput.DisposeAsync();
        if (StandardOutput != null)
            await StandardOutput.DisposeAsync();
        if (StandardError != null)
            await StandardError.DisposeAsync();
        if (DisposeOutput)
            await SourceOutput.DisposeAsync();
    }

}
