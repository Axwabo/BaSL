using System.IO.Pipelines;

await using var outStream = Console.OpenStandardOutput();
await using var outWriter = new StreamWriter(outStream);
outWriter.AutoFlush = true;
await Test();
await Test();
return;

async Task Test()
{
    var pipe1 = new Pipe();
    var pipe2 = new Pipe();

    await Task.WhenAll(
        Task.Run(() => pipe1.Reader.CopyToAsync(outStream)),
        Task.Run(async () =>
        {
            await using var writer = pipe1.Writer.AsStream();
            await pipe2.Reader.CopyToAsync(writer);
        }),
        WriteAsync()
    );

    return;

    async Task WriteAsync()
    {
        await using var writer = new StreamWriter(pipe2.Writer.AsStream());
        await writer.WriteLineAsync("hewooooo");
    }
}
