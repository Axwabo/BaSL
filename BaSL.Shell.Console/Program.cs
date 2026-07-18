using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Shell.Console;
using Directory = BaSL.FileSystems.Directory;

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

var shell = new BaSL.Console(CreateFileSystem())
{
    StandardInput = inReader,
    StandardOutput = outWriter,
    StandardError = errWriter
};
shell.CurrentDirectory = (Directory) shell.FileSystem.Resolve("/usr/bin");
return await shell.StartAsync();

FileSystem CreateFileSystem()
{
    var fs = FileSystem.CreateVirtual();
    var bin = (Directory) fs.Resolve("/usr/bin");
    bin.CreateFile("echo", Mode.Rwx).MakeExecutable(context => new Echo(context));
    bin.CreateFile("pwd", Mode.Rwx).MakeExecutable(context => new Pwd(context));
    bin.CreateFile("cd", Mode.Rwx).MakeExecutable(context => new Cd(context));
    bin.CreateFile("ls", Mode.Rwx).MakeExecutable(context => new Ls(context));
    return fs;
}
