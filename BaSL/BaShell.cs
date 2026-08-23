using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.BuiltIns;
using BaSL.Executables;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Interpreter;
using BaSL.Syntax;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;
using Path = BaSL.FileSystems.Path;

namespace BaSL;

public sealed class BaShell : App
{

    internal static readonly Dictionary<string, Executable> BuiltInCommands = new()
    {
        {"help", context => new Help(context) {Vars = context.Shell.Vars}},
        {"let", context => new Let(context) {Vars = context.Shell.Vars}},
        {"export", context => new Export(context) {Vars = context.Shell.Vars}},
        {"set", context => new Set(context) {Vars = context.Shell.Vars}},
        {"unset", context => new Unset(context) {Vars = context.Shell.Vars}}
    };

    internal static (ExecutableContext, BaShell) CreateRoot(Console console, StreamWriter standardOutput, StreamWriter standardError)
    {
        var shell = new BaShell(console, standardOutput, standardError);
        return (shell.Context, shell);
    }

    internal static (ExecutableContext, BaShell) CreateSubshell(ExecutableContext parent, ShellStatement? statement = null, UserContext? user = null)
    {
        var shell = new BaShell(parent, statement, user);
        return (shell.Context, shell);
    }

    private static void Initialize<T>() => _ = typeof(T);

    private readonly Stack<(KeywordSegment Segment, bool Skip)> _blocks = [];

    private readonly Variables _exported = [];

    private readonly Variables _local = [];

    private readonly ShellStatement? _statement;

    private CancellationTokenSource? _cts;

    static BaShell() => Initialize<DefaultArgumentParsers>();

    private BaShell(ExecutableContext context, ShellStatement? statement, UserContext? user = null) : base(null!)
    {
        _statement = statement;
        Console = context.Console;
        UserContext = user ?? context.Shell.UserContext;
        CurrentDirectory = context.WorkingDirectory;
        Context = ExecutableContext.Sub(context, this, context.FileSystem, context.Args);
        ImportEnv(context.Shell._exported);
    }

    private BaShell(Console console, StreamWriter standardOutput, StreamWriter standardError) : base(null!)
    {
        Console = console;
        UserContext = console.UserContext;
        CurrentDirectory = console.FileSystem.ResolveDirectory(console.User.Home).Unwrap();
        Context = ExecutableContext.Root(this, console, default, standardOutput, standardError);
        ImportEnv();
    }

    private int LastExitCode
    {
        set => _local["?"] = value.ToString();
    }

    internal Console Console { get; }

    public User User => UserContext.User;

    public new UserContext UserContext { get; }

    public string Hostname => Console.OperatingSystem.Hostname;

    public Directory CurrentDirectory { get; internal set; }

    private new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

    // ReSharper disable once InconsistentNaming
    public string[] PATH => _local.GetValueOrDefault("PATH", "").Split(':');

    private (Variables, Variables) Vars => (_local, _exported);

    private void ImportEnv(Variables? exported = null)
    {
        LastExitCode = 0;
        for (var i = 0; i < Context.Args.Length; i++)
            _local[i.ToString()] = Context.Args[i];
        foreach (var kvp in User.Environment)
            _local[kvp.Key] = kvp.Value;
        if (exported == null)
            return;
        foreach (var kvp in exported)
            _exported[kvp.Key] = _local[kvp.Key] = kvp.Value;
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken) => _statement is not null
        ? await ExecuteSingleAsync(_statement, cancellationToken)
        : Context.Args switch
        {
            [] or ["-i"] => await ExecuteInteractiveAsync(cancellationToken),
            [var file] => await RunAsSubshellAsync(() => ExecuteFileAsync(file, cancellationToken)),
            ["-c", var command] => await RunAsSubshellAsync(async () =>
            {
                await ExecuteAsync(command, cancellationToken);
                int.TryParse(_local.GetValueOrDefault("?", "0"), out var exitCode);
                return exitCode;
            }),
            _ => await ErrorAsync(
                """
                Invalid usage. Use either:
                basl
                basl -i
                basl -c 'command'
                basl /path/to/script
                """,
                cancellationToken
            )
        };

    private async Task<int> RunAsSubshellAsync(Func<Task<int>> execute)
    {
        var copy = Context.CopyAsync();
        var code = await execute();
        await Context.CompletePipesAsync();
        await copy;
        return code;
    }

    private async Task<int> ExecuteSingleAsync(ShellStatement statement, CancellationToken cancellationToken)
    {
        var copy = Context.CopyAsync();
        var code = await ExecuteAsync(statement, cancellationToken);
        await Context.CompletePipesAsync();
        await copy;
        return code;
    }

    private async Task<int> ExecuteAsync(ShellStatement? shellStatement, CancellationToken cancellationToken)
    {
        switch (shellStatement)
        {
            case null:
                await StandardError.WriteLineAsync("Invalid statement", cancellationToken);
                return 1;
            case StandaloneStatement {Location: AutoCommandLocation {Phrase: "true"}}:
                return 0;
            case StandaloneStatement {Location: AutoCommandLocation {Phrase: "false"}}:
                return 1;
            case StandaloneStatement standaloneStatement:
            {
                // TODO: this sucks
                await using var context = ExecutableContext.Sub(Context, this, FileSystem, standaloneStatement.Args);
                var process = Run.Execute(this, standaloneStatement.Location, context, cancellationToken);
                if (!process.Success)
                    return await ErrorAsync(standaloneStatement.Location, process.Error, cancellationToken);
                var copy = context.CopyAsync();
                var code = await process.Value;
                await copy;
                return code;
            }
            case FileStdinStatement fileStdinStatement:
            {
                var file = WorkingDirectory.ResolveFile(fileStdinStatement.SourcePath).OpenRead(UserContext);
                if (!file.Success)
                {
                    await StandardError.WriteAsync("Cannot open '", cancellationToken);
                    await StandardError.WriteAsync(fileStdinStatement.SourcePath, cancellationToken);
                    await StandardError.WriteAsync("' due to: ", cancellationToken);
                    await StandardError.WriteLineAsync(file.Error.Message);
                    return 127;
                }

                await using var stream = file.Value;
                await using var context = ExecutableContext.Stdin(Context, this, FileSystem, fileStdinStatement.Args, stream);
                var process = Run.Execute(this, fileStdinStatement.Location, context, cancellationToken);
                if (!process.Success)
                    return await ErrorAsync(fileStdinStatement.Location, process.Error, cancellationToken);
                var copy = context.CopyAsync();
                var code = await process.Value;
                await copy;
                return code;
            }
            case RedirectStatement {Source: StandaloneStatement standaloneStatement} redirectStatement:
            {
                var file = WorkingDirectory.ResolveFileOrCreate(UserContext, redirectStatement.SinkPath).OpenWrite(UserContext);
                if (!file.Success)
                {
                    await StandardError.WriteAsync("Cannot open file '", cancellationToken);
                    await StandardError.WriteAsync(redirectStatement.SinkPath, cancellationToken);
                    await StandardError.WriteAsync("' due to: ", cancellationToken);
                    await StandardError.WriteLineAsync(file.Error.Message);
                    return 127;
                }

                await using var stream = file.Value;
                if (redirectStatement.Overwrite)
                    stream.SetLength(0);
                else
                    stream.Seek(0, SeekOrigin.End);
                await using var context = ExecutableContext.Redirected(Context, Shell, FileSystem, standaloneStatement.Args, new Streams(null, stream, null));
                var process = Run.Execute(this, standaloneStatement.Location, context, cancellationToken);
                if (!process.Success)
                    return await ErrorAsync(standaloneStatement.Location, process.Error, cancellationToken);
                var copy = context.CopyAsync();
                var code = await process.Value;
                await copy;
                return code;
            }
            case PipeStatement {Source: StandaloneStatement standaloneStatement} pipeStatement:
            {
                var sourceCommand = Run.Locate(this, standaloneStatement.Location);
                if (!sourceCommand.Success)
                    return await ErrorAsync(standaloneStatement.Location, sourceCommand.Error, cancellationToken);
                var targetCommand = Run.Locate(this, pipeStatement.Location);
                if (!targetCommand.Success)
                    return await ErrorAsync(pipeStatement.Location, targetCommand.Error, cancellationToken);
                await using var source = new ExecutableContext(Shell, standaloneStatement.Args).CreatePipes();
                await using var target = new ExecutableContext(Shell, pipeStatement.Args);
                source.SubStderr(Context);
                target.PipeStdin(source);
                target.CreateStdoutPipe().SubStdout(Context);
                target.CreateStderrPipe().SubStderr(Context);
                var sourceProcess = sourceCommand.Value(source, cancellationToken);
                if (!sourceProcess.Success)
                    return await ErrorAsync(standaloneStatement.Location, sourceProcess.Error, cancellationToken);
                var targetProcess = targetCommand.Value(target, cancellationToken);
                if (!targetProcess.Success)
                    return await ErrorAsync(pipeStatement.Location, targetProcess.Error, cancellationToken);
                var copy = Task.WhenAll(source.CopyAsync(), target.CopyAsync());
                var codes = await Task.WhenAll(sourceProcess.Value, targetProcess.Value);
                await copy;
                return codes[^1];
            }
            case PipeStatement pipeStatement:
            {
                var run = new List<(CommandLocation, RunCommand, Args)>();
                ExtendableStatement? statement = pipeStatement;
                do
                {
                    var (location, args) = statement switch
                    {
                        PipeStatement {Location: var targetLocation, Args: var targetArgs} => (targetLocation, targetArgs),
                        StandaloneStatement {Location: var standaloneLocation, Args: var standaloneArgs} => (standaloneLocation, standaloneArgs),
                        _ => throw new ArgumentOutOfRangeException(nameof(statement))
                    };
                    var result = Run.Locate(this, location);
                    if (!result.Success)
                        return await ErrorAsync(location, result.Error, cancellationToken);
                    run.Add((location, result.Value, args));
                    statement = (statement as PipeStatement)?.Source;
                    // TODO: ability to redirect last
                }
                while (statement is not null);

                var contexts = new List<ExecutableContext>();
                try
                {
                    var source = new ExecutableContext(Shell, run[^1].Item3).CreatePipes();
                    source.SubStderr(Context);
                    contexts.Add(source);
                    for (var i = run.Count - 2; i >= 1; i--)
                    {
                        var args = run[i].Item3;
                        var intermediate = new ExecutableContext(Shell, args);
                        intermediate.PipeStdin(contexts[^1]);
                        intermediate.CreateStdoutPipe();
                        intermediate.CreateStderrPipe().SubStderr(Context); // TODO: concurrent writes are most likely not possible
                        contexts.Add(intermediate);
                    }

                    var target = new ExecutableContext(Shell, run[0].Item3);
                    target.PipeStdin(contexts[^1]);
                    target.CreateStdoutPipe().SubStdout(Context);
                    target.CreateStderrPipe().SubStderr(Context);
                    contexts.Add(target);

                    var copies = new List<Task>();
                    var codes = new List<Task<int>>();

                    for (var i = 0; i < contexts.Count; i++)
                    {
                        var (location, func, _) = run[contexts.Count - i - 1];
                        var execute = func(contexts[i], cancellationToken);
                        if (!execute.Success)
                            return await ErrorAsync(location, execute.Error, cancellationToken);
                        codes.Add(execute.Value);
                        copies.Add(contexts[i].CopyAsync());
                    }

                    var copy = Task.WhenAll(copies.ToArray());
                    var codeResults = await Task.WhenAll(codes.ToArray());
                    await copy;
                    return codeResults[^1];
                }
                finally
                {
                    foreach (var context in contexts)
                        await context.DisposeAsync();
                }
            }
            default:
                await StandardError.WriteAsync("Statement too complex or invalid: ", cancellationToken);
                await StandardError.WriteLineAsync(shellStatement.ToString(), cancellationToken);
                return 1;
        }
    }

    private async Task<int> ErrorAsync(CommandLocation location, Error error, CancellationToken cancellationToken)
    {
        await StandardError.WriteAsync("Cannot execute '", cancellationToken);
        await StandardError.WriteAsync(location, cancellationToken);
        await StandardError.WriteAsync("' due to: ", cancellationToken);
        await StandardError.WriteLineAsync(error.Message);
        return 127;
    }

    private async Task<int> ExecuteFileAsync(Path path, CancellationToken cancellationToken)
    {
        var file = WorkingDirectory.ResolveFile(path).OpenRead(UserContext);
        if (!file.Success)
        {
            await StandardError.WriteAsync(path.Value, cancellationToken);
            await StandardError.WriteAsync(": ", cancellationToken);
            await StandardError.WriteLineAsync(file.Error.Message, cancellationToken);
            return 127;
        }

        await using var stream = file.Value;
        var reader = new StreamReader(stream);
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            // TODO: exception handling & shi
            if (line == null)
                break;
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;
            var span = line.AsSpan().Trim();
            if (span is "exit")
                break;
            if (span.StartsWith("exit ") && int.TryParse(span["exit ".Length..].Trim(), out var exitCode))
                return exitCode;
            await ExecuteAsync(line, cancellationToken);
        }

        return 0;
    }

    private async Task<int> ExecuteInteractiveAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await StandardOutput.WriteAsync(Display.InteractivePrefix(this));
            var line = await StandardInput.ReadLineAsync();
            if (line == null)
                break;
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                continue;
            var span = line.AsSpan().Trim();
            if (span is "exit")
                break;
            if (span.StartsWith("exit ") && int.TryParse(span["exit ".Length..].Trim(), out var exitCode))
                return exitCode;
            var cts = _cts = new CancellationTokenSource();
            var token = cts.Token;
            try
            {
                await ExecuteAsync(line, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            finally
            {
                cts.Dispose();
                _cts = null;
            }
        }

        return 0;
    }

    private async Task ExecuteAsync(string line, CancellationToken token)
    {
        var statements = StatementParser.Parse(line, _local.TryGetValue, User.Home.Value);
        int index;
        var start = 0;
        do
        {
            if (_blocks.TryPeek(out var tuple) && tuple.Skip)
            {
                var skipTo = statements.FindIndex(tuple.Segment, start);
                if (skipTo == -1)
                    break;
                start = Math.Max(start, skipTo);
            }

            index = statements.FindIndex<ContinueSegment>(start);
            var end = index == -1;
            var range = end ? start.. : start..index;
            if (end && statements.Span[range].IsEmpty)
                break;
            start = index + 1;
            var controlled = false;
            if (statements.Span[range.Start] is KeywordSegment {Keyword: var keyword})
            {
                if (await ControlAsync(keyword, tuple, statements, range, token))
                    continue;
                controlled = true;
            }

            var span = statements.Span[range];
            var statement = StatementParser.CreateStatement(controlled ? span[1..] : span);
            var code = LastExitCode = await ExecuteAsync(statement, token);
            if (!end && statements.Span[range.End] is ContinueSegment @continue && @continue.Exit(code))
                break;
        }
        while (index != -1);
    }

    private async Task<bool> ControlAsync(Keyword keyword, (KeywordSegment Segment, bool Skip) tuple, ReadOnlyMemory<Segment> statements, Range range, CancellationToken token)
    {
        switch (keyword, tuple.Segment?.Keyword, tuple.Skip)
        {
            case (Keyword.If, _, _):
            {
                switch (statements.Span[range][1..])
                {
                    case [KeywordSegment {Keyword: Keyword.BeginCondition}, ArgsSegment {Args: var condition}, KeywordSegment {Keyword: Keyword.EndCondition}]:
                        if (Conditions.IsTrueComplex(condition, CurrentDirectory) is { } @true)
                            return _blocks.Skip(@true);
                        break;
                    case var syntax when StatementParser.CreateStatement(syntax) is { } statement:
                        return _blocks.Skip(LastExitCode = await ExecuteAsync(statement, token));
                }

                await StandardError.WriteLineAsync("Unsupported if statement condition, defaulting to false");
                return _blocks.Skip(false);
            }
            case (Keyword.Then, Keyword.Then, true):
                _blocks.Transition(KeywordSegment.Then, false);
                return false;
            case (Keyword.Then, Keyword.Else, true):
                return false;
            case (Keyword.Else, Keyword.Then, false):
                _blocks.Transition(KeywordSegment.EndIf, true);
                return true;
            case (Keyword.Else, Keyword.Else, true):
                _blocks.Transition(KeywordSegment.Else, false);
                return false;
            // TODO: what should the keyword check be
            case (Keyword.EndIf, Keyword.Then or Keyword.Else or Keyword.EndIf, _):
                _blocks.Pop();
                return true;
            default:
                await StandardError.WriteAsync("Unexpected token '", token);
                await StandardError.WriteAsync(keyword.Token, token);
                await StandardError.WriteLineAsync('\'');
                return false;
        }
    }

    internal GetFileResult ResolveFromPath(FileSystemEntryName arg)
    {
        foreach (var directoryPath in PATH)
        {
            var directory = FileSystem.ResolveDirectory(directoryPath);
            if (!directory.Success)
                continue;
            var file = directory.Value.ResolveFile(arg);
            if (file.Success)
                return file;
        }

        return GetEntryError.NotFound;
    }

    public bool Cancel()
    {
        if (_cts == null)
            return false;
        _cts.Cancel();
        return true;
    }

}
