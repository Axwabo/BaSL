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

    private readonly ExecutableContext _context;

    private BaShell? _shell;

    public Console(OperatingSystem operatingSystem, string username, StreamWriter standardOutput, StreamWriter standardError)
    {
        OperatingSystem = operatingSystem;
        UserContext = new UserContext(operatingSystem.Users[username]);
        CurrentDirectory = FileSystem.ResolveDirectory(User.Home).Value!;
        _context = ExecutableContext.Root(this, FileSystem, ReadOnlyMemory<string>.Empty, standardOutput, standardError);
    }

    public OperatingSystem OperatingSystem { get; }

    public UserContext UserContext { get; }

    public User User => UserContext.User;

    public FileSystem FileSystem => OperatingSystem.FileSystem;

    public StreamWriter StandardInput => _context.DestinationInput;

    public Directory CurrentDirectory { get; internal set; }

    public async Task<int> StartAsync()
    {
        await using var context = _context;
        _shell = new BaShell(context);
        return await _shell.ExecuteAsync(CancellationToken.None);
    }

    public void TerminateCurrentProcess() => _shell?.Cancel();

}
