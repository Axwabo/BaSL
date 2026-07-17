using BaSL;
using BaSL.Executables;
using BaSL.FileSystems;

await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();
await using var stderr = Console.OpenStandardError();

var context = new ExecutableContext(FileSystem.CreateVirtual(), stdin, stdout, stderr, args);
var shell = new Shell(context);
return await shell.ExecuteAsync();
