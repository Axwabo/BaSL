using BaSL.FileSystems;
using BaSL.Shell;

await using var stdin = Console.OpenStandardInput();
await using var stdout = Console.OpenStandardOutput();
await using var stderr = Console.OpenStandardError();

var shell = new Shell
{
    FileSystem = FileSystem.CreateVirtual(),
    StandardInput = stdin,
    StandardOutput = stdout,
    StandardError = stderr
};
return await shell.ExecuteAsync();
