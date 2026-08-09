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
using BaSL.Syntax;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;
using File = BaSL.FileSystems.File;
using Path = BaSL.FileSystems.Path;
using RunCommand = System.Func<BaSL.Executables.ExecutableContext, System.Threading.CancellationToken, BaSL.Result<System.Threading.Tasks.Task<int>, BaSL.Error>>;

namespace BaSL;

using LocateCommandResult = Result<RunCommand, Error>;

public sealed class BaShell : App
{

    private static readonly Dictionary<string, Action<BaShell, ExecutableContext>> BuiltInCommands = new()
    {
        {"clear", (_, _) => System.Console.Clear()},
        {
            "set", (shell, context) =>
            {
                if (context.Args.Length == 2)
                    shell._variables[context.Args[0]] = context.Args[1];
            }
        },
        {
            "unset", (shell, context) =>
            {
                if (context.Args.Length == 1)
                    shell._variables.Remove(context.Args[0]);
            }
        },
        {
            "export", (shell, context) =>
            {
                if (context.Args.Length == 2)
                    shell._exported[context.Args[0]] = shell._variables[context.Args[0]] = context.Args[1];
            }
        }
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

    internal static (ExecutableContext, BaShell) CreateSubshell(ExecutableContext parent, ShellStatement? statement=null)
    {
        var shell = new BaShell(parent, statement);
        return (shell.Context, shell);
    }

    private readonly Dictionary<string, string> _exported = [];

    private readonly ShellStatement? _statement;

    private readonly Dictionary<string, string> _variables = [];

    private CancellationTokenSource? _cts;

    private BaShell(ExecutableContext context, ShellStatement? statement) : base(null!)
    {
        _statement = statement;
        Context = ExecutableContext.Sub(context, this, context.FileSystem, context.Args);
        UserContext = context.Shell.UserContext;
        Hostname = context.Shell.Hostname;
        CurrentDirectory = context.WorkingDirectory;
        ImportEnv();
        foreach (var kvp in context.Shell._exported)
            _exported[kvp.Key] = _variables[kvp.Key] = kvp.Value;
    }

    private BaShell(Console console, StreamWriter standardOutput, StreamWriter standardError) : base(null!)
    {
        UserContext = console.UserContext;
        Hostname = console.OperatingSystem.Hostname; // TODO: auto-update
        CurrentDirectory = console.FileSystem.ResolveDirectory(console.User.Home).Unwrap();
        Context = ExecutableContext.Root(this, console.FileSystem, default, standardOutput, standardError);
        ImportEnv();
    }

    private int LastExitCode
    {
        set => _variables["$"] = value.ToString();
    }

    public User User => UserContext.User;

    public new UserContext UserContext { get; }

    public string Hostname { get; }

    public Directory CurrentDirectory { get; internal set; }

    private new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

    private void ImportEnv()
    {
        foreach (var kvp in User.Environment)
            _variables[kvp.Key] = kvp.Value;
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        => _statement is null
            ? await ExecuteInteractiveAsync()
            : await ExecuteAsync(_statement, cancellationToken);

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
                return 0;
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
                await using var source = new ExecutableContext(Shell, FileSystem, standaloneStatement.Args).CreatePipes();
                await using var target = new ExecutableContext(Shell, FileSystem, pipeStatement.Args);
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
                    var source = new ExecutableContext(Shell, FileSystem, run[^1].Item3).CreatePipes();
                    source.SubStderr(Context);
                    contexts.Add(source);
                    for (var i = run.Count - 2; i >= 1; i--)
                    {
                        var args = run[i].Item3;
                        var intermediate = new ExecutableContext(Shell, FileSystem, args);
                        intermediate.PipeStdin(contexts[^1]);
                        intermediate.CreateStdoutPipe();
                        intermediate.CreateStderrPipe().SubStderr(Context); // TODO: concurrent writes are most likely not possible
                        contexts.Add(intermediate);
                    }

                    var target = new ExecutableContext(Shell, FileSystem, run[0].Item3);
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

    private async Task<int> ExecuteInteractiveAsync()
    {
        while (true)
        {
            await StandardOutput.WriteAsync($"{User.Username}@{Shell.Hostname}:{FormatCurrentDirectory()}{(User.IsSuperuser ? "# " : "$ ")}");
            var line = await StandardInput.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
                continue;
            if (line.AsSpan().Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                return 0;
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
    }

    private async Task ExecuteAsync(string line, CancellationToken token)
    {
        var statements = StatementParser.Parse(line, _variables.TryGetValue, User.Home.Value);
        foreach (var (statement, @continue) in statements)
        {
            var code = LastExitCode = await ExecuteAsync(statement, token);
            var success = code == 0;
            if (@continue switch
                {
                    Continue.Always => false,
                    Continue.OnFailure => success,
                    Continue.OnSuccess => !success,
                    _ => true
                })
                break;
        }
    }

    private LocateCommandResult Locate(CommandLocation location) => location switch
    {
        PathCommandLocation {FullPath: var path} => Execute(path),
        AutoCommandLocation {Phrase: var path} when Path.IsExplicitRelativeOrAbsolute(path) => Execute(path),
        AutoCommandLocation {Phrase: var name} when BuiltInCommands.TryGetValue(name, out var action) => (RunCommand) ((context, _) =>
        {
            action(this, context);
            return Complete();

            async Task<int> Complete()
            {
                await context.CompletePipesAsync();
                return 0;
            }
        }),
        AutoCommandLocation {Phrase: var name} when ResolveFromPath(name) is {Success: true, Value: var file} => Execute(file),
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
        var path = _variables.GetValueOrDefault("PATH", "").Split(':');
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

    private string FormatCurrentDirectory()
    {
        var path = Shell.CurrentDirectory.FullPath.Value.AsSpan();
        var home = User.Home.Value.AsSpan();
        if (!path.StartsWith(home))
            return Shell.CurrentDirectory.FullPath.Value;
        Span<char> span = stackalloc char[path.Length - home.Length + 1];
        span[0] = '~';
        path[home.Length..].CopyTo(span[1..]);
        return span.ToString();
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
