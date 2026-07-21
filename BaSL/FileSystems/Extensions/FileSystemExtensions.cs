using System;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemExtensions
{

    extension(FileSystem fileSystem)
    {

        public GetEntryResult Resolve(Path path)
        {
            FileSystemEntry entry = fileSystem.Root;
            foreach (var s in path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries))
            {
                if (entry is not Directory directory)
                    break;
                var result = directory.GetEntry(s);
                if (!result.Success)
                    return result.Error;
                entry = result.Value;
            }

            return entry;
        }

        public GetDirectoryResult ResolveDirectory(Path path) => fileSystem.Resolve(path).AsDirectory();

        public GetFileResult ResolveFile(Path path) => fileSystem.Resolve(path).AsFile();

    }

}
