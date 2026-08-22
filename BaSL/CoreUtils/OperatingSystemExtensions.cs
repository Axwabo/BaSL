using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Users;

namespace BaSL.CoreUtils;

public static class OperatingSystemExtensions
{

    private static readonly Modes BinaryModes = Directory.DefaultFileModes with {Others = Mode.Rx};

    private static Task Install(OperatingSystem system, UserContext ctx)
    {
        var bin = system.FileSystem.Root.CreateDirectories(ctx, Path.Binaries).Unwrap();
        Create("basl", context => BaShell.CreateSubshell(context).Item2);
        Create("mkdir", context => new Mkdir(context));
        Create("rmdir", context => new Rmdir(context));
        Create("rm", context => new Rm(context));
        Create("echo", context => new Echo(context));
        Create("env", context => new Env(context));
        Create("pwd", context => new Pwd(context));
        Create("cd", context => new Cd(context));
        Create("ls", context => new Ls(context));
        Create("chmod", context => new Chmod(context));
        Create("cat", context => new Cat(context));
        Create("touch", context => new Touch(context));
        Create("bytes", context => new Bytes(context));
        Create("whoami", context => new WhoAmI(context));
        Create("sleep", context => new Sleep(context));
        Create("sudo", context => new Sudo(context));
        bin.Link(ctx, "bash", bin.FullPath / "basl");
        system.FileSystem.Root.Link(ctx, "bin", bin.FullPath).Unwrap();
        return Task.CompletedTask;

        void Create(FileSystemEntryName name, Executable executable) => bin.CreateBinary(ctx, name, executable);
    }

    extension(OperatingSystem operatingSystem)
    {

        public async Task InstallCoreUtilsAsync() => await operatingSystem.SudoAsync(Install);

    }

    extension(Directory directory)
    {

        public FileSystemError? CreateBinary(UserContext context, FileSystemEntryName name, Executable executable) => directory.CreateFile(context, name, BinaryModes).MakeExecutable(context, executable);

    }

}
