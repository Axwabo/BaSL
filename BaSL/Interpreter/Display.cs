using System;

namespace BaSL.Interpreter;

public static class Display
{

    public static string FormatCurrentDirectory(BaShell shell)
    {
        var path = shell.CurrentDirectory.FullPath.Value.AsSpan();
        var home = shell.User.Home.Value.AsSpan();
        if (!path.StartsWith(home))
            return shell.CurrentDirectory.FullPath.Value;
        Span<char> span = stackalloc char[path.Length - home.Length + 1];
        span[0] = '~';
        path[home.Length..].CopyTo(span[1..]);
        return span.ToString();
    }

    public static string InteractivePrefix(BaShell shell) => $"{shell.User.Username}@{shell.Hostname}:{FormatCurrentDirectory(shell)}{(shell.User.IsSuperuser ? "# " : "$ ")}";

}
