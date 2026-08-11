using BaSL;
using BaSL.CoreUtils;
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
        await using (var writer = userHome.OpenTextWrite("amogus.txt", context).Unwrap())
        {
            await writer.WriteLineAsync("Hello World!");
        }

        var shebang = userHome.CreateFile(context, "shebang.sh").Unwrap();
        await using (var writer = shebang.OpenTextWrite(context).Unwrap())
        {
            await writer.WriteLineAsync("#!/usr/bin/basl");
            await writer.WriteLineAsync("echo Hello from subshell!");
            await writer.WriteLineAsync("if [[ $USER == 'root' ]]");
            await writer.WriteLineAsync("    echo You are root");
            await writer.WriteLineAsync("else");
            await writer.WriteLineAsync("    echo You are NOT root");
            await writer.WriteLineAsync("fi");
            await writer.WriteLineAsync("echo End of if-else statement");
        }

        shebang.ChmodPlusX(context);
    });
    return system;
}
