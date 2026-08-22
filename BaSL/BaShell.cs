using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Interpreter;
using BaSL.Syntax;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;
using File = BaSL.FileSystems.File;
using Path = BaSL.FileSystems.Path;
using RunCommand = System.Func<BaSL.Executables.ExecutableContext, System.Threading.CancellationToken, BaSL.Result<System.Threading.Tasks.Task<int>, BaSL.Error>>;

namespace BaSL;

using BulitInCommand = Func<BaShell, ExecutableContext, Task<int>>;
using LocateCommandResult = Result<RunCommand, Error>;

public sealed class BaShell : App
{

    private static readonly Dictionary<string, BulitInCommand> BuiltInCommands = new()
    {
        {"clear", Sync(System.Console.Clear)},
        {
            "set", Sync((shell, context) =>
            {
                if (context.Args.Length == 2)
                    shell._variables[context.Args[0]] = context.Args[1];
            })
        },
        {
            "unset", Sync((shell, context) =>
            {
                if (context.Args.Length == 1)
                    shell._variables.Remove(context.Args[0]);
            })
        },
        {
            "export", Sync((shell, context) =>
            {
                if (context.Args.Length == 2)
                    shell._exported[context.Args[0]] = shell._variables[context.Args[0]] = context.Args[1];
            })
        },
        {
            "help", async (shell, context) =>
            {
                if (context.Args.IsEmpty)
                {
                    foreach (var directoryPath in shell.PATH)
                    {
                        var directory = shell.FileSystem.ResolveDirectory(directoryPath);
                        if (!directory.Success)
                            continue;
                        foreach (var file in directory.Value.EnumerateFiles())
                            if (file.Executable != null && file.Metadata.CanExecute(shell.User))
                                await shell.StandardOutput.WriteLineAsync(file.Name);
                    }

                    await shell.StandardOutput.WriteLineAsync("clear");
                    await shell.StandardOutput.WriteLineAsync("set");
                    await shell.StandardOutput.WriteLineAsync("unset");
                    await shell.StandardOutput.WriteLineAsync("export");
                    await shell.StandardOutput.WriteLineAsync("exit");
                    await shell.StandardOutput.WriteLineAsync("help");
                    return 0;
                }

                var result = shell.ResolveFromPath(context.Args[0]);
                if (!result.Success)
                {
                    await shell.StandardError.WriteLineAsync(result.Error.Message);
                    return 1;
                }

                if (result.Value.Executable is not { } executable)
                {
                    await shell.StandardError.WriteLineAsync("Not an executable");
                    return 1;
                }

                await shell.StandardOutput.WriteLineAsync(result.Value.FullPath.Value);
                var app = executable(context);
                if (app is IHelpProvider provider)
                    await provider.DisplayHelpAsync(CancellationToken.None);
                else
                    await shell.StandardOutput.WriteLineAsync("No help available :(");
                return 0;
            }
        }
    };

    private static BulitInCommand Sync(Action<BaShell, ExecutableContext> execute) => (shell, context) =>
    {
        execute(shell, context);
        return Task.FromResult(0);
    };

    private static BulitInCommand Sync(Action execute) => (_, _) =>
    {
        execute();
        return Task.FromResult(0);
    };

    private static RunCommand Execute(File file) => (context, token) =>
    {
        var execute = file.Execute(context, token);
        return !execute.Success
            ? execute.Error
            : Result<Task<int>, Error>.CreateSuccess(RunAndComplete(execute.Value));

        async Task<int> RunAndComplete(Process process)
        {
            var code = await process.WaitForExitAsync();
            await context.CompletePipesAsync();
            return code;
        }
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

    private readonly Stack<(KeywordSegment Segment, bool Skip)> _blocks = [];

    private readonly Variables _exported = [];

    private readonly ShellStatement? _statement;

    private readonly Variables _variables = [];

    private CancellationTokenSource? _cts;

    static BaShell()
    {
        ArgumentParser<bool>.Delegate = bool.TryParse;
        ArgumentParser<float>.Delegate = float.TryParse;
        ArgumentParser<double>.Delegate = double.TryParse;
        ArgumentParser<int>.Delegate = int.TryParse;
        ArgumentParser<byte>.Delegate = byte.TryParse;
    }

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
        set => _variables["?"] = value.ToString();
    }

    internal Console Console { get; }

    public User User => UserContext.User;

    public new UserContext UserContext { get; }

    public string Hostname => Console.OperatingSystem.Hostname;

    public Directory CurrentDirectory { get; internal set; }

    private new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

    // ReSharper disable once InconsistentNaming
    public string[] PATH => _variables.GetValueOrDefault("PATH", "").Split(':');

    private void ImportEnv(Variables? exported = null)
    {
        LastExitCode = 0;
        for (var i = 0; i < Context.Args.Length; i++)
            _variables[i.ToString()] = Context.Args[i];
        foreach (var kvp in User.Environment)
            _variables[kvp.Key] = kvp.Value;
        if (exported == null)
            return;
        foreach (var kvp in exported)
            _exported[kvp.Key] = _variables[kvp.Key] = kvp.Value;
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_statement is not null)
            return await ExecuteSingleAsync(_statement, cancellationToken);
        if (Context.Args.IsEmpty)
            return await ExecuteInteractiveAsync(cancellationToken);
        var copy = Context.CopyAsync();
        var code = await ExecuteFileAsync(Context.Args[0], cancellationToken);
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
                var process = Execute(standaloneStatement.Location, context, cancellationToken);
                if (!process.Success)
                    return await WriteExecuteErrorAsync(standaloneStatement.Location, process.Error, cancellationToken);
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
                var process = Execute(fileStdinStatement.Location, context, cancellationToken);
                if (!process.Success)
                    return await WriteExecuteErrorAsync(fileStdinStatement.Location, process.Error, cancellationToken);
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
                var process = Execute(standaloneStatement.Location, context, cancellationToken);
                if (!process.Success)
                    return await WriteExecuteErrorAsync(standaloneStatement.Location, process.Error, cancellationToken);
                var copy = context.CopyAsync();
                var code = await process.Value;
                await copy;
                return code;
            }
            case PipeStatement {Source: StandaloneStatement standaloneStatement} pipeStatement:
            {
                var sourceCommand = Locate(standaloneStatement.Location);
                if (!sourceCommand.Success)
                    return await WriteExecuteErrorAsync(standaloneStatement.Location, sourceCommand.Error, cancellationToken);
                var targetCommand = Locate(pipeStatement.Location);
                if (!targetCommand.Success)
                    return await WriteExecuteErrorAsync(pipeStatement.Location, targetCommand.Error, cancellationToken);
                await using var source = new ExecutableContext(Shell, standaloneStatement.Args).CreatePipes();
                await using var target = new ExecutableContext(Shell, pipeStatement.Args);
                source.SubStderr(Context);
                target.PipeStdin(source);
                target.CreateStdoutPipe().SubStdout(Context);
                target.CreateStderrPipe().SubStderr(Context);
                var sourceProcess = sourceCommand.Value(source, cancellationToken);
                if (!sourceProcess.Success)
                    return await WriteExecuteErrorAsync(standaloneStatement.Location, sourceProcess.Error, cancellationToken);
                var targetProcess = targetCommand.Value(target, cancellationToken);
                if (!targetProcess.Success)
                    return await WriteExecuteErrorAsync(pipeStatement.Location, targetProcess.Error, cancellationToken);
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
                    var result = Locate(location);
                    if (!result.Success)
                        return await WriteExecuteErrorAsync(location, result.Error, cancellationToken);
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
                            return await WriteExecuteErrorAsync(location, execute.Error, cancellationToken);
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

    private async Task<int> WriteExecuteErrorAsync(CommandLocation location, Error error, CancellationToken cancellationToken)
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
        var statements = StatementParser.Parse(line, _variables.TryGetValue, User.Home.Value);
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
            if (statements.Span[range.End] is ContinueSegment @continue && @continue.Exit(code))
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
                return _blocks.Transition(KeywordSegment.Then, false);
            case (Keyword.Then, Keyword.Else, true):
                return false;
            case (Keyword.Else, Keyword.Then, false):
                return _blocks.Transition(KeywordSegment.EndIf, true);
            case (Keyword.Else, Keyword.Else, true):
                return _blocks.Transition(KeywordSegment.Else, false);
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

    private LocateCommandResult Locate(CommandLocation location) => location switch
    {
        PathCommandLocation {FullPath: var path} => Execute(path),
        AutoCommandLocation {Phrase: var path} when Path.IsExplicitRelativeOrAbsolute(path) => Execute(path),
        AutoCommandLocation {Phrase: var name} when BuiltInCommands.TryGetValue(name, out var action) => (RunCommand) ((context, _) =>
        {
            var task = action(this, context);
            return Complete();

            async Task<int> Complete()
            {
                var code = await task;
                await context.CompletePipesAsync();
                return code;
            }
        }),
        AutoCommandLocation {Phrase: var name} => ResolveFromPath(name) switch
        {
            {Success: true, Value: var file} => Execute(file),
            {Success: false, Error: NotFoundError} => CommandError.NotFound,
            {Error: var error} => error
        },
        _ => CommandError.NotFound
    };

    private LocateCommandResult Execute(Path path)
    {
        var file = WorkingDirectory.ResolveFile(path);
        return file.Success ? Execute(file.Value) : file.Error;
    }

    private Result<Task<int>, Error> Execute(CommandLocation location, ExecutableContext context, CancellationToken cancellationToken)
    {
        var locate = Locate(location);
        return locate.Success ? locate.Value(context, cancellationToken) : locate.Error;
    }

    private GetFileResult ResolveFromPath(FileSystemEntryName arg)
    {
        var path = PATH;
        foreach (var directoryPath in path)
        {
            var directory = FileSystem.ResolveDirectory(directoryPath);
            if (!directory.Success)
                continue;
            var file = directory.Value.GetFile(arg);
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

// TODO
    private sealed record CommandError() : Error("Command not found")
    {

        public static Error NotFound { get; } = new CommandError();

    }

}
