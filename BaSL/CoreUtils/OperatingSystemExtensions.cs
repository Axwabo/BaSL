using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Users;

namespace BaSL.CoreUtils;

public static class OperatingSystemExtensions
{

    private static Task Install(OperatingSystem system, UserContext ctx)
    {
        var bin = system.FileSystem.Root.CreateDirectories(Path.Binaries).Value!;
        CreateBinary("basl", context => new BaShell(context));
        CreateBinary("mkdir", context => new Mkdir(context));
        CreateBinary("rmdir", context => new Rmdir(context));
        CreateBinary("rm", context => new Rm(context));
        CreateBinary("echo", context => new Echo(context));
        CreateBinary("pwd", context => new Pwd(context));
        CreateBinary("cd", context => new Cd(context));
        CreateBinary("ls", context => new Ls(context));
        CreateBinary("cat", context => new Cat(context));
        CreateBinary("bytes", context => new Bytes(context));
        CreateBinary("whoami", context => new WhoAmI(context));
        CreateBinary("sleep", context => new Sleep(context));
        return Task.CompletedTask;

        void CreateBinary(FileSystemEntryName name, Executable executable)
        {
            var file = bin.CreateFile(name, Mode.Rwx).Value!;
            file.MakeExecutable(ctx, executable);
            file.Metadata.ChangeMode(file.Metadata.Modes with {Others = Mode.Rx});
        }
    }

    extension(OperatingSystem operatingSystem)
    {

        public async Task InstallCoreUtilsAsync() => await operatingSystem.SudoAsync(Install);

    }

}
