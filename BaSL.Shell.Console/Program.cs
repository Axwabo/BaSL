using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Dev;
using BaSL.FileSystems.Extensions;
using BaSL.Shell.Console;
using BaSL.Users;
using Console = System.Console;
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

var console = new BaSL.Console(CreateSystem(), "user")
{
    StandardInput = inReader,
    StandardOutput = outWriter,
    StandardError = errWriter
};
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
    var bin = rootFs.Root.CreateDirectory("usr").Value!.CreateDirectory("bin").Value!;
    ((IMountSupport) rootFs.Root).Mount(new DevFileSystem(system.Root), "dev", system.Root, new Modes(Mode.Rwx, Mode.Rwx, Mode.Read));
    CreateBinary("echo", context => new Echo(context));
    CreateBinary("pwd", context => new Pwd(context));
    CreateBinary("cd", context => new Cd(context));
    CreateBinary("ls", context => new Ls(context));
    CreateBinary("cat", context => new Cat(context));
    CreateBinary("bytes", context => new Bytes(context));
    CreateBinary("whoami", context => new WhoAmI(context));

    var user = system.CreateUser("user");
    var userHome = system.FileSystem.ResolveDirectory(user.Home).Value!;
    using var writer = new StreamWriter(userHome.CreateFile("amogus.txt").Value!.Open(ctx, OpenMode.ReadWrite).Value!);
    writer.WriteLineAsync("Hello World!");
    return system;

    void CreateBinary(FileSystemEntryName name, Executable executable)
    {
        var file = bin.CreateFile(name, Mode.Rwx).Value!;
        file.MakeExecutable(ctx, executable);
        file.Metadata.ChangeMode(file.Metadata.Modes with {Others = Mode.Rx});
    }
}
