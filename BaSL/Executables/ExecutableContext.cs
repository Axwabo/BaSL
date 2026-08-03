using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            IsRoot = true,
            _sourceOutput = standardOutput,
            _sourceError = standardError
        }.CreatePipes();

    internal static ExecutableContext Sub(ExecutableContext parent, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        var context = new ExecutableContext(console, fileSystem, args).CreatePipes();
        SubStdout(parent, context);
        SubStderr(parent, context);
        return context;
    }

    private static void SubStdout(ExecutableContext parent, ExecutableContext context)
    {
        if (parent.StandardOutput != null)
            context._copy.Add((context.DestinationOutput!, parent.SourceOutput, context.StandardOutput!, false, "stdout"));
    }

    private static void SubStderr(ExecutableContext parent, ExecutableContext context)
    {
        if (parent.StandardError != null)
            context._copy.Add((context.DestinationError!, parent.SourceError, context.StandardError!, false, "stderr"));
    }

    internal static ExecutableContext Piped(ExecutableContext source, ExecutableContext parent, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args)
    {
        var context = new ExecutableContext(console, fileSystem, args)
            .CreatePipes()
            .PipeStdin(source.StandardOutput);
        SubStdout(parent, context); // TODO: where
        return context;
    }

    internal static ExecutableContext Redirected(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, Streams streams)
    {
        var context = new ExecutableContext(console, fileSystem, args);
        if (streams.StandardInput is { } stdin)
        {
            context._sourceInput = new StreamReader(stdin);
            context._completables.Add(stdin);
        }
        else
            context.CreateStdinPipe().PipeStdin(source); // TODO: idk bruh :sob:

        if (streams.StandardOutput is { } stdout)
        {
            context._sourceOutput = new StreamWriter(stdout) {AutoFlush = true};
            context._completables.Add(stdout);
        }
        else
            SubStdout(source, context.CreateStdoutPipe());

        if (streams.StandardError is { } stderr)
        {
            context._sourceError = new StreamWriter(stderr) {AutoFlush = true};
            context._completables.Add(stderr);
        }
        else
            SubStderr(source, context.CreateStderrPipe());

        return context;
    }

    private static async Task CopyAsync(StreamReader source, StreamWriter destination, PipeWrapper cancellation, bool dispose)
    {
        var token = cancellation.CancellationToken;
        try
        {
            await source.BaseStream.CopyToAsync(destination.BaseStream, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
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
    private readonly HashSet<IAsyncDisposable> _completables = [];

    private readonly List<(StreamReader, StreamWriter, PipeWrapper, bool, string)> _copy = [];

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

    internal PipeWrapper? StandardInput { get; private set; }
    internal PipeWrapper? StandardOutput { get; private set; }
    internal PipeWrapper? StandardError { get; private set; }

    internal StreamReader SourceInput => ThrowIfNull(_sourceInput);

    internal StreamWriter SourceOutput => ThrowIfNull(_sourceOutput);

    internal StreamWriter SourceError => ThrowIfNull(_sourceError);

    internal StreamWriter? DestinationInput => _destinationInput;

    internal StreamReader? DestinationOutput => _destinationOutput;

    internal StreamReader? DestinationError => _destinationError;

    internal bool IsRoot { get; private set; }

    internal ExecutableContext PipeStdin(PipeWrapper? source)
    {
        if (source != null)
            _sourceInput = source.Reader; // TODO: probably use this in other places instead of copying
        return this;
    }

    internal ExecutableContext PipeStdin(ExecutableContext source)
    {
        /*if (source._sourceInput != null && DestinationInput != null)
            _copy.Add((source._sourceInput, DestinationInput, StandardInput!, false, "stdin"));*/
        return this;
    }

    private PipeWrapper CreatePipe([NotNull] ref StreamReader? reader, [NotNull] ref StreamWriter? writer)
    {
        var pipe = new PipeWrapper();
        reader ??= pipe.Reader;
        writer ??= pipe.Writer;
        _disposables.Add(pipe);
        return pipe;
    }

    internal ExecutableContext CreatePipes() => CreateStdinPipe().CreateStdoutPipe().CreateStderrPipe();

    internal ExecutableContext CreateStdinPipe()
    {
        StandardInput ??= CreatePipe(ref _sourceInput, ref _destinationInput);
        return this;
    }

    internal ExecutableContext CreateStdoutPipe()
    {
        StandardOutput ??= CreatePipe(ref _destinationOutput, ref _sourceOutput);
        _completables.Add(_sourceOutput!);
        return this;
    }

    internal ExecutableContext CreateStderrPipe()
    {
        StandardError ??= CreatePipe(ref _destinationError, ref _sourceError);
        _completables.Add(_sourceError!);
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
                var (reader, writer, pipeWrapper, dispose, tag) = _copy[i];
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

    public async ValueTask CompletePipesAsync()
    {
        foreach (var disposable in _completables)
            await disposable.DisposeAsync();
    }

}
