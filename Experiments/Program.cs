using System.IO.Pipelines;

var pipe = new Pipe();
await using (var writer = new StreamWriter(pipe.Writer.AsStream()))
{
    writer.AutoFlush = true;
    await writer.WriteLineAsync("Hello world!");
    await pipe.Writer.CompleteAsync();
}

using var reader = new StreamReader(pipe.Reader.AsStream());
Console.WriteLine("First:");
Console.WriteLine(await reader.ReadLineAsync());
Console.WriteLine("Second:");
var line = await reader.ReadLineAsync();
Console.WriteLine(line ?? "Ended");
