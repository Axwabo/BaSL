using BaSL.FileSystems;
using BaSL.FileSystems.Dev;
using BaSL.FileSystems.Extensions;
using BaSL.Shell.Console;
using Console = System.Console;
using Directory = BaSL.FileSystems.Directory;
using OperatingSystem = BaSL.OperatingSystem;

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

var console = new BaSL.Console(CreateSystem())
{
    StandardInput = inReader,
    StandardOutput = outWriter,
    StandardError = errWriter
};
console.CurrentDirectory = (Directory) console.FileSystem.Resolve("/usr/bin");
Console.CancelKeyPress += (_, eventArgs) =>
{
    console.TerminateCurrentProcess();
    eventArgs.Cancel = true;
};
return await console.StartAsync();

OperatingSystem CreateSystem()
{
    var system = new OperatingSystem();
    var rootFs = system.FileSystem;
    var userFs = FileSystem.CreateVirtual();
    using (var writer = new StreamWriter(userFs.Root.CreateFile("amogus.txt").Open()))
    {
        writer.WriteLineAsync("Hello World!");
    }

    var bin = rootFs.Root.CreateDirectory("usr").CreateDirectory("bin");
    var home = rootFs.Root.CreateDirectory("home");
    ((IMountSupport) home).Mount(userFs, "user");
    ((IMountSupport) rootFs.Root).Mount(new DevFileSystem(), "dev");
    bin.CreateFile("echo", Mode.Rwx).MakeExecutable(context => new Echo(context));
    bin.CreateFile("pwd", Mode.Rwx).MakeExecutable(context => new Pwd(context));
    bin.CreateFile("cd", Mode.Rwx).MakeExecutable(context => new Cd(context));
    bin.CreateFile("ls", Mode.Rwx).MakeExecutable(context => new Ls(context));
    bin.CreateFile("cat", Mode.Rwx).MakeExecutable(context => new Cat(context));
    bin.CreateFile("bytes", Mode.Rwx).MakeExecutable(context => new Bytes(context));
    return system;
}
