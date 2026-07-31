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
using Path = BaSL.FileSystems.Path;

namespace BaSL;

public sealed class BaShell : App
{

    private static readonly Dictionary<string, Action> BuiltInCommands = new()
    {
        {
            "clear", System.Console.Clear
        }
    };

    private static Result<Func<Task<int>>, Error> WaitForExit(Process process) => Result<Func<Task<int>>, Error>.CreateSuccess(process.WaitForExitAsync);

    private readonly ShellStatement? _statement;

    private CancellationTokenSource? _cts;

    public BaShell(ExecutableContext context) : base(context)
    {
        foreach (var kvp in context.Console.User.Environment)
            ExportedVariables[kvp.Key] = kvp.Value;
    }

    public BaShell(ExecutableContext context, ShellStatement statement) : this(context) => _statement = statement;

    private int? LastExitCode
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
                var command = ExecuteCommand(standaloneStatement.Location, context, cancellationToken);
                if (!command.Success)
                {
                    await StandardError.WriteAsync("Cannot execute '", cancellationToken);
                    await StandardError.WriteAsync(standaloneStatement.Location, cancellationToken);
                    await StandardError.WriteAsync("' due to: ", cancellationToken);
                    await StandardError.WriteLineAsync(command.Error.Message);
                    return 127;
                }

                var copy = context.CopyAsync();
                var code = await command.Value();
                await context.CompletePipesAsync();
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
                var command = ExecuteCommand(standaloneStatement.Location, context, cancellationToken);
                if (!command.Success)
                {
                    await StandardError.WriteAsync("Cannot execute '", cancellationToken);
                    await StandardError.WriteAsync(standaloneStatement.Location, cancellationToken);
                    await StandardError.WriteAsync("' due to: ", cancellationToken);
                    await StandardError.WriteLineAsync(command.Error.Message);
                    return 127;
                }

                var copy = context.CopyAsync();
                var code = await command.Value();
                await context.CompletePipesAsync();
                await copy;
                return code;
            }
            case PipeStatement pipeStatement:
            default:
                await StandardError.WriteAsync("Statement too complex or invalid: ", cancellationToken);
                await StandardError.WriteLineAsync(shellStatement.ToString(), cancellationToken);
                return 1;
        }
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

    private Result<Func<Task<int>>, Error> ExecuteCommand(CommandLocation location, ExecutableContext context, CancellationToken token) => location switch
    {
        PathCommandLocation {FullPath: var path} => Execute(path, context, token),
        AutoCommandLocation {Phrase: var path} when Path.IsExplicitRelativeOrAbsolute(path) => Execute(path, context, token),
        AutoCommandLocation {Phrase: var name} when BuiltInCommands.TryGetValue(name, out var action) => Result<Func<Task<int>>, Error>.CreateSuccess(() =>
        {
            action();
            return Task.FromResult(0);
        }),
        AutoCommandLocation {Phrase: var name} when ResolveFromPath(name).Execute(context, token) is {Success: true, Value: var process} => WaitForExit(process),
        _ => CommandError.NotFound
    };

    private Result<Func<Task<int>>, Error> Execute(Path path, ExecutableContext context, CancellationToken token)
    {
        var command = WorkingDirectory.ResolveFile(path).Execute(context, token);
        return command.Success
            ? WaitForExit(command.Value)
            : command.Error;
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
