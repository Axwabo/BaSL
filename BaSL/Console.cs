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

    private readonly BaShell _shell;

    public Console(OperatingSystem operatingSystem, string username)
    {
        OperatingSystem = operatingSystem;
        UserContext = new UserContext(operatingSystem.Users[username]);
        CurrentDirectory = FileSystem.ResolveDirectory(User.Home).Value!;
        _context = ExecutableContext.Root(this, FileSystem, ReadOnlyMemory<string>.Empty);
        _shell = new BaShell(_context);
    }

    public OperatingSystem OperatingSystem { get; }

    public UserContext UserContext { get; }

    public User User => UserContext.User;

    public FileSystem FileSystem => OperatingSystem.FileSystem;

    public StreamReader StandardInput => _context.SourceInput;

    public StreamWriter StandardOutput => _context.SourceOutput;

    public StreamWriter StandardError => _context.SourceError;

    public Directory CurrentDirectory { get; internal set; }

    public async Task<int> StartAsync()
    {
        await using var context = _context;
        var copy = context.CopyAsync();
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
