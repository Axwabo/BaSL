using BaSL.Shell.Console;
using Console = System.Console;

await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();
await using var stderr = Console.OpenStandardError();

await using var outWriter = new StreamWriter(stdout);
outWriter.AutoFlush = true;
await using var errWriter = new StreamWriter(stderr);
outWriter.AutoFlush = true;

Console.SetOut(outWriter);
Console.SetError(errWriter);

var console = await Setup.CreateConsoleAsync(args, outWriter, errWriter);
using var cts = new CancellationTokenSource();
_ = InputBuffer.ReadAsync(console, cts.Token);
return await console.StartAsync();
