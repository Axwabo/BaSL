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
            "export", (shell, context) =>
            {
                if (context.Args.Length == 2)
                    shell.ExportedVariables[context.Args[0]] = context.Args[1];
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

    private readonly ShellStatement? _statement;

    private CancellationTokenSource? _cts;

    public BaShell(ExecutableContext context) : base(context)
    {
        foreach (var kvp in context.Console.User.Environment)
            ExportedVariables[kvp.Key] = kvp.Value;
    }

    public BaShell(ExecutableContext context, ShellStatement statement) : this(context) => _statement = statement;

    private int LastExitCode
    {
        set => ExportedVariables["$"] = value.ToString();
    }

    public Dictionary<string, string> ExportedVariables { get; } = [];

    private User User => Console.User;

    private new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

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
            case StandaloneStatement standaloneStatement:
            {
                // TODO: this sucks
                await using var context = ExecutableContext.Sub(Context, Console, FileSystem, standaloneStatement.Args);
                var process = Execute(standaloneStatement.Location, context, cancellationToken);
                if (!process.Success)
                {
                    await StandardError.WriteAsync("Cannot execute '", cancellationToken);
                    await StandardError.WriteAsync(standaloneStatement.Location, cancellationToken);
                    await StandardError.WriteAsync("' due to: ", cancellationToken);
                    await StandardError.WriteLineAsync(process.Error.Message);
                    return 127;
                }

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
                await using var context = ExecutableContext.Redirected(Context, Console, FileSystem, standaloneStatement.Args, new Streams(null, stream, null));
                var process = Execute(standaloneStatement.Location, context, cancellationToken);
                if (!process.Success)
                {
                    await StandardError.WriteAsync("Cannot execute '", cancellationToken);
                    await StandardError.WriteAsync(standaloneStatement.Location, cancellationToken);
                    await StandardError.WriteAsync("' due to: ", cancellationToken);
                    await StandardError.WriteLineAsync(process.Error.Message);
                    return 127;
                }

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
                var targetCommand = Locate(pipeStatement.TargetLocation);
                if (!targetCommand.Success)
                    return await WriteExecuteErrorAsync(pipeStatement.TargetLocation, targetCommand.Error, cancellationToken);
                await using var source = new ExecutableContext(Console, FileSystem, standaloneStatement.Args).CreatePipes();
                await using var target = new ExecutableContext(Console, FileSystem, pipeStatement.TargetArgs);
                source.SubStderr(Context);
                target.PipeStdin(source);
                target.CreateStdoutPipe().SubStdout(Context);
                target.CreateStderrPipe().SubStderr(Context);
                var sourceProcess = sourceCommand.Value(source, cancellationToken);
                if (!sourceProcess.Success)
                    return await WriteExecuteErrorAsync(standaloneStatement.Location, sourceProcess.Error, cancellationToken);
                var targetProcess = targetCommand.Value(target, cancellationToken);
                if (!targetProcess.Success)
                    return await WriteExecuteErrorAsync(pipeStatement.TargetLocation, targetProcess.Error, cancellationToken);
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
                        PipeStatement {TargetLocation: var targetLocation, TargetArgs: var targetArgs} => (targetLocation, targetArgs),
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
                    var source = new ExecutableContext(Console, FileSystem, run[^1].Item3).CreatePipes();
                    source.SubStderr(Context);
                    contexts.Add(source);
                    for (var i = run.Count - 2; i >= 1; i--)
                    {
                        var args = run[i].Item3;
                        var intermediate = new ExecutableContext(Console, FileSystem, args);
                        intermediate.PipeStdin(contexts[^1]);
                        intermediate.CreateStdoutPipe();
                        intermediate.CreateStderrPipe().SubStderr(Context); // TODO: concurrent writes are most likely not possible
                    }

                    var target = new ExecutableContext(Console, FileSystem, run[0].Item3);
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
            await StandardOutput.WriteAsync($"{User.Username}@{Console.OperatingSystem.Hostname}:{FormatCurrentDirectory()}{(User.IsSuperuser ? "# " : "$ ")}");
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
        var statements = StatementParser.Parse(line, ExportedVariables.TryGetValue);
        foreach (var statement in statements)
            LastExitCode = await ExecuteAsync(statement, token);
    }

    private LocateCommandResult Locate(CommandLocation location) => location switch
    {
        PathCommandLocation {FullPath: var path} => Execute(path),
        AutoCommandLocation {Phrase: var path} when Path.IsExplicitRelativeOrAbsolute(path) => Execute(path),
        AutoCommandLocation {Phrase: var name} when BuiltInCommands.TryGetValue(name, out var action) => (RunCommand) ((context, _) =>
        {
            action(this, context);
            return Task.FromResult(0);
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
        var path = ExportedVariables.GetValueOrDefault("PATH", "").Split(':');
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
        var path = Console.CurrentDirectory.FullPath.Value.AsSpan();
        var home = User.Home.Value.AsSpan();
        if (!path.StartsWith(home))
            return Console.CurrentDirectory.FullPath.Value;
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
