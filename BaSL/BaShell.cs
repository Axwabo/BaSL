using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Syntax;
using BaSL.Users;

namespace BaSL;

public sealed class BaShell : App
{

    private CancellationTokenSource? _cts;

    public BaShell(ExecutableContext context) : base(context)
    {
        foreach (var kvp in context.Console.User.Environment)
            ExportedVariables[kvp.Key] = kvp.Value;
    }

    private int? LastExitCode
    {
        set => ExportedVariables["$"] = value.ToString();
    }

    public Dictionary<string, string> ExportedVariables { get; } = [];

    private User User => Console.User;

    private new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
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
                await await ExecuteAsync(line, token);
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

    private async Task<Task> ExecuteAsync(string line, CancellationToken token)
    {
        var statements = StatementParser.Parse(line, ExportedVariables.TryGetValue);
        switch (statements)
        {
            case [{Type: StatementType.Simple} oneSimple]:
            {
                return await ExecuteSimpleAsync(oneSimple.Args, token);
            }
            case [{Type: StatementType.RedirectStandardOutputOverwrite or StatementType.RedirectStandardOutputAppend} statement, {Type: StatementType.Simple, Args: {Length: not 0} targetFile}]:
                return await ExecuteToFileAsync(statement.Args, targetFile.Span[0], statement.Type == StatementType.RedirectStandardOutputOverwrite, token);
            default:
                await StandardOutput.WriteLineAsync("Statement too complex or invalid", token);
                return Task.CompletedTask;
        }
    }

    private async Task<Task> ExecuteSimpleAsync(ReadOnlyMemory<string> args, CancellationToken token)
    {
        await using var context = ExecutableContext.Piped(Context, Console, FileSystem, args[1..]);
        return await ExecuteAsync(args, context, token);
    }

    private async Task<Task> ExecuteAsync(ReadOnlyMemory<string> args, ExecutableContext context, CancellationToken token)
    {
        var result = ResolveFromPath(args.Span[0]).Execute(context, token);
        if (result is not {Success: true, Value: var process})
        {
            LastExitCode = 127; // TODO: uhhhhhh sure..?
            await StandardError.WriteLineAsync(result.Error.Message); // TODO: fix sync
            return Task.CompletedTask;
        }

        var copy = context.CopyAsync(!Context.IsRoot);
        LastExitCode = await process.WaitForExitAsync();
        return copy;
    }

    private async Task<Task> ExecuteToFileAsync(ReadOnlyMemory<string> args, string outputFile, bool overwrite, CancellationToken token)
    {
        var fileResult = WorkingDirectory.ResolveFileOrCreate(UserContext, outputFile).Open(UserContext, OpenMode.ReadWrite);
        if (!fileResult.Success)
        {
            await StandardOutput.WriteAsync("Cannot open file '", token);
            await StandardOutput.WriteAsync(outputFile, token);
            await StandardOutput.WriteAsync("': ", token);
            await StandardOutput.WriteLineAsync(fileResult.Error.Message, token);
            return Task.CompletedTask;
        }

        await using var stream = fileResult.Value;
        if (overwrite)
            stream.SetLength(0);
        else
            stream.Seek(0, SeekOrigin.End);
        await using var context = ExecutableContext.Sunken(Context, Console, FileSystem, args[1..], new StreamWriter(stream), StreamWriter.Null); // TODO: where to pipe sterr?
        return await ExecuteAsync(args, context, token);
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

}
