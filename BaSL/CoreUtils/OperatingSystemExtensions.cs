using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Users;

namespace BaSL.CoreUtils;

public static class OperatingSystemExtensions
{

    private static readonly Modes BinaryModes = Directory.DefaultFileModes with {Others = Mode.Rx};

    private static Task Install(OperatingSystem system, UserContext ctx)
    {
        var bin = system.FileSystem.Root.CreateDirectories(ctx, Path.Binaries).Unwrap();
        CreateBinary("basl", context => BaShell.CreateSubshell(context).Item2);
        CreateBinary("mkdir", context => new Mkdir(context));
        CreateBinary("rmdir", context => new Rmdir(context));
        CreateBinary("rm", context => new Rm(context));
        CreateBinary("echo", context => new Echo(context));
        CreateBinary("env", context => new Env(context));
        CreateBinary("pwd", context => new Pwd(context));
        CreateBinary("cd", context => new Cd(context));
        CreateBinary("ls", context => new Ls(context));
        CreateBinary("chmod", context => new Chmod(context));
        CreateBinary("cat", context => new Cat(context));
        CreateBinary("touch", context => new Touch(context));
        CreateBinary("bytes", context => new Bytes(context));
        CreateBinary("whoami", context => new WhoAmI(context));
        CreateBinary("sleep", context => new Sleep(context));
        system.FileSystem.Root.Link(ctx, "bin", bin.FullPath).Unwrap();
        return Task.CompletedTask;

        void CreateBinary(FileSystemEntryName name, Executable executable) => bin.CreateFile(ctx, name, BinaryModes).MakeExecutable(ctx, executable);
    }

    extension(OperatingSystem operatingSystem)
    {

        public async Task InstallCoreUtilsAsync() => await operatingSystem.SudoAsync(Install);

    }

}
