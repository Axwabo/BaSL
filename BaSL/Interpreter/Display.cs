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

    public static string InteractivePrefix(BaShell shell) => $"\e[1;36m{shell.User.Username}@{shell.Hostname}\e[0m:\e[1;36m{FormatCurrentDirectory(shell)}\e[0m{(shell.User.IsSuperuser ? "# " : "$ ")}";

}
