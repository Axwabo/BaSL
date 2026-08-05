using BaSL;
using BaSL.CoreUtils;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Shell.Console;
using Console = System.Console;
using OperatingSystem = BaSL.OperatingSystem;

await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();
await using var stderr = Console.OpenStandardError();

await using var outWriter = new StreamWriter(stdout);
outWriter.AutoFlush = true;
await using var errWriter = new StreamWriter(stderr);
outWriter.AutoFlush = true;

Console.SetOut(outWriter);
Console.SetError(errWriter);

var console = new BaSL.Console(await CreateSystemAsync(errWriter), "user", outWriter, errWriter);
using var cts = new CancellationTokenSource();
_ = InputBuffer.ReadAsync(console, cts.Token);
return await console.StartAsync();

async Task<OperatingSystem> CreateSystemAsync(StreamWriter err)
{
    var system = new OperatingSystem {Hostname = "OwOS"};
    await system.InstallCoreUtilsAsync();
    var user = system.CreateUser("user").Unwrap();
    await system.SudoAsync(async (operatingSystem, context) =>
    {
        await AutoMount.Mount(args, operatingSystem, context, err);
        var userHome = operatingSystem.FileSystem.ResolveDirectory(user.Home).Unwrap();
        await using var writer = new StreamWriter(userHome.CreateFile(context, "amogus.txt").Open(context, OpenMode.ReadWrite).Unwrap());
        await writer.WriteLineAsync("Hello World!");
    });
    return system;
}
