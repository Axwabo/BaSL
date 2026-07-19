using System;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemExtensions
{

    extension(FileSystem fileSystem)
    {

        public FileSystemEntry Resolve(Path path)
        {
            FileSystemEntry entry = fileSystem.Root;
            foreach (var s in path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries))
            {
                if (entry is not Directory directory)
                    break;
                entry = directory.GetEntry(s);
            }

            return entry;
        }

    }

}
