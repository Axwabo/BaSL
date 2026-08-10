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

    internal static ExecutableContext Root(BaShell shell, Console console, Args args, StreamWriter standardOutput, StreamWriter standardError)
        => new ExecutableContext(shell, args)
        {
            IsRoot = true,
            _sourceOutput = standardOutput,
            _sourceError = standardError
        }.CreatePipes();

    internal static ExecutableContext Sub(ExecutableContext parent, BaShell shell, FileSystem fileSystem, Args args, bool copyStdout = true)
    {
        var context = new ExecutableContext(shell, args).CreatePipes();
        if (copyStdout)
            context.SubStdout(parent);
        context.SubStderr(parent);
        return context;
    }

    internal static ExecutableContext Piped(ExecutableContext source, ExecutableContext parent, BaShell shell, FileSystem fileSystem, Args args)
    {
        var context = new ExecutableContext(shell, args)
            .CreatePipes()
            .PipeStdin(source.StandardOutput);
        context.SubStdout(parent); // TODO: where
        return context;
    }

    internal static ExecutableContext Stdin(ExecutableContext parent, BaShell shell, FileSystem fileSystem, Args args, Stream standardInput)
    {
        var context = new ExecutableContext(shell, args);
        context.CreateStdoutPipe().SubStdout(parent);
        context.CreateStderrPipe().SubStderr(parent);
        context._sourceInput = new StreamReader(standardInput);
        context._completables.Add(standardInput);
        return context;
    }

    internal static ExecutableContext Redirected(ExecutableContext source, BaShell shell, FileSystem fileSystem, Args args, Streams streams)
    {
        var context = new ExecutableContext(shell, args);
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
            context.CreateStdoutPipe().SubStdout(source);

        if (streams.StandardError is { } stderr)
        {
            context._sourceError = new StreamWriter(stderr) {AutoFlush = true};
            context._completables.Add(stderr);
        }
        else
            context.CreateStderrPipe().SubStderr(source);

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

    internal ExecutableContext(Console console, BaShell shell, Args args)
    {
        Shell = shell;
        WorkingDirectory = shell.CurrentDirectory;
        Args = args;
        Console = console;
    }

    internal ExecutableContext(BaShell shell, Args args) : this(shell.Console, shell, args)
    {
    }

    internal BaShell Shell { get; }
    internal Console Console { get; }
    internal FileSystem FileSystem => Console.FileSystem;
    internal Directory WorkingDirectory { get; }
    internal Args Args { get; }

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

    public void SubStdout(ExecutableContext parent)
    {
        if (parent.StandardOutput != null)
            _copy.Add((DestinationOutput!, parent.SourceOutput, StandardOutput!, false, "stdout"));
    }

    public void SubStderr(ExecutableContext parent)
    {
        if (parent.StandardError != null)
            _copy.Add((DestinationError!, parent.SourceError, StandardError!, false, "stderr"));
    }

    internal ExecutableContext PipeStdin(PipeWrapper? source)
    {
        if (source != null)
            _sourceInput = source.Reader; // TODO: probably use this in other places instead of copying
        return this;
    }

    internal ExecutableContext PipeStdin(ExecutableContext source)
    {
        source._completables.Add(source.StandardOutput!.Writer);
        _sourceInput = source.StandardOutput!.Reader;
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
