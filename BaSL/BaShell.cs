using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Users;

namespace BaSL;

public sealed class BaShell : App
{

    private CancellationTokenSource? _cts;

    private int? _lastExitCode;

    public BaShell(ExecutableContext context) : base(context)
    {
        foreach (var kvp in context.Console.User.Environment)
            ExportedVariables[kvp.Key] = kvp.Value;
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
        var args = Expand(line).ToArray();
        await using var context = ExecutableContext.Piped(Context, Console, FileSystem, args.AsMemory(1));
        var result = ResolveFromPath(args[0]).Execute(context, token);
        if (result is not {Success: true, Value: var process})
        {
            _lastExitCode = 127; // TODO: uhhhhhh sure..?
            await StandardError.WriteLineAsync(result.Error.Message); // TODO: fix sync
            return Task.CompletedTask;
        }

        var copy = context.CopyAsync(!Context.IsRoot);
        _lastExitCode = await process.WaitForExitAsync();
        return copy;
    }

    private List<string> Expand(string line)
    {
        var list = new List<string>();
        var wrap = Wrap.None;
        var sb = new StringBuilder();
        foreach (var c in line)
            if (c == '"')
                End(Wrap.DoubleQuotes);
            else if (c == '\'')
                End(Wrap.SingleQuotes);
            else if (char.IsWhiteSpace(c) && wrap is not (Wrap.SingleQuotes or Wrap.DoubleQuotes))
                End(Wrap.None);
            else if (wrap == Wrap.None && c == '$')
                wrap = Wrap.Variable;
            else
                sb.Append(c);
        End(wrap);

        return list;

        void End(Wrap newWrap)
        {
            if (sb.Length == 0)
            {
                wrap = newWrap == wrap ? Wrap.None : newWrap;
                return;
            }

            switch (wrap)
            {
                case Wrap.None or Wrap.SingleQuotes or Wrap.DoubleQuotes:
                    list.Add(sb.ToString());
                    break;
                case Wrap.Variable when sb is ['?'] && _lastExitCode != null:
                    list.Add(_lastExitCode.ToString());
                    break;
                case Wrap.Variable when ExportedVariables.TryGetValue(sb.ToString(), out var env):
                    list.Add(env);
                    break;
            }

            sb.Clear();
            wrap = newWrap == wrap ? Wrap.None : newWrap;
        }
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

    public void Cancel() => _cts?.Cancel();

}

file enum Wrap
{

    None,
    Variable,
    SingleQuotes,
    DoubleQuotes

}
