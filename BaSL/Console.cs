using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Syntax;
using BaSL.Users;
using Directory = BaSL.FileSystems.Directory;
using Path = BaSL.FileSystems.Path;

namespace BaSL;

public sealed class Console
{

    private readonly ExecutableContext _context;

    private BaShell? _shell;

    public Console(OperatingSystem operatingSystem, string username, StreamWriter standardOutput, StreamWriter standardError)
    {
        OperatingSystem = operatingSystem;
        UserContext = new UserContext(operatingSystem.Users[username]);
        CurrentDirectory = FileSystem.ResolveDirectory(User.Home).Unwrap();
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
        Path path = "/home/user/among.txt";
        var statement = StandaloneStatement.FromPath(Path.Binaries / "echo", "hello world") > path;
        _shell = new BaShell(context, statement);
        var code = await _shell.ExecuteAsync(CancellationToken.None);
        await _context.SourceOutput.WriteAsync("Contents of ");
        await _context.SourceOutput.WriteLineAsync(path);
        using var reader = FileSystem.ResolveFile(path).OpenReadOrNull(UserContext);
        await reader.BaseStream.CopyToAsync(_context.SourceOutput.BaseStream);
        return code;
    }

    public bool TerminateCurrentProcess() => _shell?.Cancel() ?? false;

}
