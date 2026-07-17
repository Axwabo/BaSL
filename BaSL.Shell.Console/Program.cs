using BaSL.FileSystems;

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

var shell = new BaSL.Console(FileSystem.CreateVirtual())
{
    StandardInput = inReader,
    StandardOutput = outWriter,
    StandardError = errWriter
};
return await shell.ExecuteAsync();
