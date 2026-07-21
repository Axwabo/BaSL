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

    private CancellationTokenSource? _cts;

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
        var binaries = FileSystem.ResolveDirectory("/usr/bin").Value!;
        var prefix = User.IsSuperuser ? "# " : "$ ";
        while (true)
        {
            await StandardOutput.WriteAsync(prefix);
            var line = await StandardInput.ReadLineAsync();
            if (string.IsNullOrEmpty(line))
                continue;
            if (line.AsSpan().Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                return 0;
            var cts = _cts = new CancellationTokenSource();
            var token = cts.Token;
            try
            {
                var args = line.Split();
                var context = new RootExecutableContext(ExecutableContext.Piped(this, FileSystem, args.AsMemory()[1..]), StandardInput, StandardOutput, StandardError);
                var fileResult = binaries.GetFile(args[0]);
                if (!fileResult.Success)
                {
                    await StandardOutput.WriteLineAsync(fileResult.Error.Message);
                    continue;
                }

                var executeResult = fileResult.Value.Execute(context, token);
                if (executeResult is {Success: true, Value: var process})
                    await process.WaitForExitAsync();
                else
                    await StandardOutput.WriteLineAsync(executeResult.Error.Message); // TODO: fix sync
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

    public void TerminateCurrentProcess() => _cts?.Cancel();

}
