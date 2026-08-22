using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;
using BaSL.Users;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;

namespace BaSL.Terminal;

public static class AutoMount
{

    private static readonly Modes Modes = new(Mode.Rx, 0, Mode.Rx);

    public static async Task Mount(string[] args, OperatingSystem operatingSystem, UserContext context, StreamWriter err)
    {
        var media = operatingSystem.FileSystem.Root.CreateDirectory(context, "media", Modes).Unwrap();
        foreach (var se in args)
        {
            if (se.Split('=') is not [var name, var path])
                continue;
            if (!FileSystemEntryName.IsValid(name))
            {
                await err.WriteAsync("Invalid mount name: ");
                await err.WriteLineAsync(name);
                continue;
            }

            if (!Directory.Exists(path))
            {
                await err.WriteAsync("Directory doesn't exist: ");
                await err.WriteLineAsync(path);
                continue;
            }

            var fs = FileSystem.CreateVirtual(context);
            var result = media.Mount(context, fs, name);
            if (!result.Success)
            {
                await err.WriteAsync("Cannot mount '");
                await err.WriteAsync(name);
                await err.WriteAsync("' due to: ");
                await err.WriteLineAsync(result.Error.Message);
                continue;
            }

            var directory = result.Value;
            // TODO: recursive
            foreach (var entry in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(path, entry).Replace('\\', '/');
                var parentPath = new FileSystems.Path(relative).Parent;
                var parent = parentPath.IsEmpty
                    ? directory
                    : directory.CreateDirectories(context, parentPath).Unwrap();
                await using var realFile = File.OpenRead(entry);
                await using var virtualFile = parent.CreateFile(context, Path.GetFileName(entry)).OpenWrite(context).Unwrap();
                await realFile.CopyToAsync(virtualFile);
            }
        }
    }

}
