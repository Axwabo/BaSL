using System;
using System.Collections.Generic;
using System.Linq;
using BaSL.FileSystems.Errors;

namespace BaSL.FileSystems.Extensions;

public static class DirectoryExtensions
{

    extension(Directory directory)
    {

        public IEnumerable<File> EnumerateFiles() => directory.EnumerateEntries().OfType<File>();

        public IEnumerable<Directory> EnumerateDirectories() => directory.EnumerateEntries().OfType<Directory>();

        public IEnumerable<FileSystemEntry> EnumerateEntriesRecursive()
        {
            foreach (var entry in directory.EnumerateEntries())
            {
                yield return entry;
                if (entry is not Directory subdirectory)
                    continue;
                foreach (var subEntry in subdirectory.EnumerateEntriesRecursive())
                    yield return subEntry;
            }
        }

        public CreateDirectoryResult CreateDirectories(Path path)
        {
            var current = directory;
            foreach (var s in path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                var result = current.CreateDirectory(s);
                if (!result.Success)
                    return result;
                current = result.Value;
            }

            return current;
        }

        public GetDirectoryResult GetParent()
        {
            var pathSpan = directory.FullPath.Value.AsSpan();
            var slash = pathSpan.LastIndexOf('/');
            return slash != -1
                ? directory.FileSystem.ResolveDirectory(pathSpan[..slash].ToString())
                : GetEntryError.NotFound;
        }

        public GetEntryResult Resolve(Path relativeOrAbsolute)
            => directory.FileSystem.Resolve(relativeOrAbsolute.ToAbsolute(directory.FullPath));

        public GetDirectoryResult ResolveDirectory(Path relativeOrAbsolute)
            => directory.Resolve(relativeOrAbsolute).AsDirectory();

        public GetFileResult ResolveFile(Path relativeOrAbsolute)
            => directory.Resolve(relativeOrAbsolute).AsFile();

    }

}
