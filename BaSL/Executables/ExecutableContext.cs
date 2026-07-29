using System;
using System.Collections.Generic;
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
        }.CreatePipes();

    internal static ExecutableContext Piped(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
        => new(console, fileSystem, args)
        {
        };

    internal static ExecutableContext Redirected(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, Streams streams)
    {
        var context = new ExecutableContext(console, fileSystem, args);
        return context;
    }

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

    private readonly List<(StreamReader, StreamWriter, PipeWrapper, bool)> _copy = [];

    private readonly List<IDisposable> _disposables = [];
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

    internal bool IsRoot => Parent == null;

    private PipeWrapper CreatePipe(ref StreamReader? reader, ref StreamWriter? writer)
    {
        var pipe = new PipeWrapper();
        reader ??= pipe.Reader;
        writer ??= pipe.Writer;
        _disposables.Add(pipe);
        return pipe;
    }

    private ExecutableContext CreatePipes() => CreateStdinPipe().CreateStdoutPipe().CreateStderrPipe();

    private ExecutableContext CreateStdinPipe()
    {
        StandardInput = CreatePipe(ref _sourceInput, ref _destinationInput);
        return this;
    }

    private ExecutableContext CreateStdoutPipe()
    {
        StandardOutput = CreatePipe(ref _destinationOutput, ref _sourceOutput);
        return this;
    }

    private ExecutableContext CreateStderrPipe()
    {
        StandardError = CreatePipe(ref _destinationError, ref _sourceError);
        return this;
    }

    internal async Task CopyAsync()
    {
        if (_copy.Count == 0)
            return;
        try
        {
            var copy = new Task[_copy.Count];
            for (var i = 0; i < copy.Length; i++)
            {
                var (reader, writer, pipeWrapper, dispose) = _copy[i];
                copy[i] = CopyAsync(reader, writer, pipeWrapper, dispose);
            }

            await Task.WhenAll(copy);
        }
        catch (OperationCanceledException) when (_disposed)
        {
        }
    }

    internal async ValueTask DisposeAsync()
    {
        _disposed = true;
        foreach (var disposable in _disposables)
            if (disposable is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                disposable.Dispose();
    }

}
