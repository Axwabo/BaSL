using System;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemExtensions
{

    extension(FileSystem fileSystem)
    {

        public FileSystemEntry Resolve(Path path)
        {
            var directory = fileSystem.Root;
            foreach (var s in path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries))
                directory = directory.GetDirectory(s);
            return directory;
        }

    }

}
