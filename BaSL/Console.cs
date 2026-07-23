using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL;

public sealed class Console
{

    private BaShell? _shell;

    public Console(OperatingSystem operatingSystem, string username)
    {
        OperatingSystem = operatingSystem;
        UserContext = new UserContext(operatingSystem.Users[username]);
        CurrentDirectory = FileSystem.ResolveDirectory(User.Home).Value!;
    }

    public OperatingSystem OperatingSystem { get; }

    public UserContext UserContext { get; }

    public User User => UserContext.User;

    public FileSystem FileSystem => OperatingSystem.FileSystem;

    public required StreamReader StandardInput { get; init; }

    public required StreamWriter StandardOutput { get; init; }

    public required StreamWriter StandardError { get; init; }

    public Directory CurrentDirectory { get; internal set; }

    public async Task<int> StartAsync()
    {
        await using var context = ExecutableContext.Root(this, FileSystem, ReadOnlyMemory<string>.Empty, StandardInput, StandardOutput, StandardError);
        _shell = new BaShell(context);
        var copy = Task.Run(context.CopyAsync);
        try
        {
            return await _shell.ExecuteAsync(CancellationToken.None);
        }
        finally
        {
            await copy;
        }
    }

    public void TerminateCurrentProcess() => _shell?.Cancel();

}
