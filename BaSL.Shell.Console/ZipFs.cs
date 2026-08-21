using System.IO.Compression;
using System.Text;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems.Extensions;
using Directory = BaSL.FileSystems.Directory;
using File = BaSL.FileSystems.File;

namespace BaSL.Shell.Console;

[Help("Zips the directory at the given path, and saves it in the physical directory BaSL is running in.")]
public sealed partial class ZipFs : App
{

    public ZipFs(ExecutableContext context) : base(context)
    {
    }

    [Execute]
    private async Task<int> Zip(string? path = null, CancellationToken cancellationToken = default)
    {
        await using var stream = System.IO.File.Create($"zipfs-{DateTimeOffset.Now:yyyy-MM-dd'_'HH'-'mm'-'ss}.zip");
        await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Create, false, Encoding.UTF8, cancellationToken);
        Directory directory;
        if (string.IsNullOrEmpty(path))
            directory = WorkingDirectory;
        else
        {
            var result = WorkingDirectory.ResolveDirectory(path);
            if (!result.Success)
            {
                await StandardOutput.WriteLineAsync(result.Error.Message, cancellationToken);
                return 1;
            }

            directory = result.Value;
        }

        var start = directory.FullPath.Length;
        foreach (var entry in directory.EnumerateEntriesRecursive())
        {
            if (entry is not File file)
                continue;
            await StandardOutput.WriteLineAsync(entry.FullPath, cancellationToken);
            var open = file.OpenRead(UserContext);
            if (!open.Success)
            {
                await StandardOutput.WriteLineAsync(open.Error.Message, cancellationToken);
                continue;
            }

            await using var fileStream = open.Value;
            var zipEntry = archive.CreateEntry(file.FullPath.Value[start..]);
            await using var zipStream = await zipEntry.OpenAsync(cancellationToken);
            await fileStream.CopyToAsync(zipStream, cancellationToken);
        }

        return 0;
    }

}
