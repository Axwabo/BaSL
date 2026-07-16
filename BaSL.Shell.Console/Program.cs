using BaSL.Shell;

await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();
await using var stderr = Console.OpenStandardError();

var shell = new Shell(stdin, stdout, stderr);
return await shell.Execute();
