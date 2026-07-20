using BaSL.FileSystems;
using BaSL.FileSystems.Dev;
using BaSL.FileSystems.Extensions;
using BaSL.Shell.Console;
using BaSL.Users;
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

var console = new BaSL.Console(CreateSystem(), "root")
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
    var ctx = new UserContext(system.Root);
    var rootFs = system.FileSystem;
    using (var writer = new StreamWriter(rootFs.Root.CreateFile("amogus.txt").Open(ctx)))
    {
        writer.WriteLineAsync("Hello World!");
    }

    var bin = rootFs.Root.CreateDirectory("usr").CreateDirectory("bin");
    ((IMountSupport) rootFs.Root).Mount(new DevFileSystem(system.Root), "dev", system.Root, new Modes(Mode.Rwx, Mode.Rwx, Mode.Read));
    bin.CreateFile("echo", Mode.Rwx).MakeExecutable(ctx, context => new Echo(context));
    bin.CreateFile("pwd", Mode.Rwx).MakeExecutable(ctx, context => new Pwd(context));
    bin.CreateFile("cd", Mode.Rwx).MakeExecutable(ctx, context => new Cd(context));
    bin.CreateFile("ls", Mode.Rwx).MakeExecutable(ctx, context => new Ls(context));
    bin.CreateFile("cat", Mode.Rwx).MakeExecutable(ctx, context => new Cat(context));
    bin.CreateFile("bytes", Mode.Rwx).MakeExecutable(ctx, context => new Bytes(context));
    return system;
}
