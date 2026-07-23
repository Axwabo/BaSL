using BaSL.CoreUtils;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Users;
using Console = System.Console;
using OperatingSystem = BaSL.OperatingSystem;

await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();
await using var stderr = Console.OpenStandardError();

using var inReader = new StreamReader(stdin);
await using var outWriter = new StreamWriter(stdout);
outWriter.AutoFlush = true;
await using var errWriter = new StreamWriter(stderr);
outWriter.AutoFlush = true;

Console.SetIn(inReader);
Console.SetOut(outWriter);
Console.SetError(errWriter);

var console = new BaSL.Console(await CreateSystemAsync(), "user")
{
    StandardInput = inReader,
    StandardOutput = outWriter,
    StandardError = errWriter
};
Console.CancelKeyPress += (_, eventArgs) =>
{
    console.TerminateCurrentProcess();
    eventArgs.Cancel = true;
};
return await console.StartAsync();

async Task<OperatingSystem> CreateSystemAsync()
{
    var system = new OperatingSystem {Hostname = "OwOS"};
    await system.InstallCoreUtilsAsync();
    var user = system.CreateUser("user");
    var userHome = system.FileSystem.ResolveDirectory(user.Home).Value!;
    await using var writer = new StreamWriter(userHome.CreateFile("amogus.txt").Open(new UserContext(user), OpenMode.ReadWrite).Value!);
    await writer.WriteLineAsync("Hello World!");
    return system;
}
