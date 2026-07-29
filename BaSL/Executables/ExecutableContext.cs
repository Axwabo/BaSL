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
            SourceOutput = standardOutput,
            SourceError = standardError
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

    private static T CheckFinalized<T>(T? returnValue) => returnValue ?? throw new InvalidOperationException("Context has not yet been initialized, this should not happen!");

    private bool _disposed;
    private StreamReader? _sourceInput;

    private ExecutableContext(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = console.CurrentDirectory;
        Args = args;
    }

    private ExecutableContext? Parent { get; init; }
    internal PipeWrapper? StandardInput { get; private set; }
    internal PipeWrapper? StandardOutput { get; private set; }
    internal PipeWrapper? StandardError { get; init; }
    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }

    internal StreamReader SourceInput => CheckFinalized(_sourceInput);

    internal StreamWriter SourceOutput
    {
        get => CheckFinalized(field);
        private set;
    } = null!;

    internal StreamWriter SourceError
    {
        get => CheckFinalized(field);
        private set;
    } = null!;

    internal StreamWriter DestinationInput
    {
        get => CheckFinalized(field);
        private set;
    } = null!;

    internal StreamReader DestinationOutput
    {
        get => CheckFinalized(field);
        private set;
    } = null!;

    internal StreamReader DestinationError
    {
        get => CheckFinalized(field);
        private set;
    } = null!;

    private bool DisposeOutput { get; init; }
    internal bool IsRoot => Parent == null;

    private ExecutableContext CreateStdinPipe()
    {
        StandardInput = new PipeWrapper();
        SourceInput = StandardInput.Reader;
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
        await StandardInput.DisposeAsync();
        await StandardOutput.DisposeAsync();
        await StandardError.DisposeAsync();
        if (DisposeOutput)
            await SourceOutput.DisposeAsync();
    }

}
