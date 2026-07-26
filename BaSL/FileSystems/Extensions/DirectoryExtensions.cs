using System;
using System.Collections.Generic;
using System.Linq;
using BaSL.FileSystems.Errors;
using BaSL.Users;

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

        public CreateDirectoryResult CreateDirectories(UserContext context, Path path)
        {
            var current = directory;
            foreach (var s in path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                var result = current.CreateDirectory(context, s);
                if (!result.Success)
                    return result;
                current = result.Value;
            }

            return current;
        }

        public GetEntryResult Resolve(Path relativeOrAbsolute)
            => directory.FileSystem.Resolve(relativeOrAbsolute.ToAbsolute(directory.FullPath));

        public GetDirectoryResult ResolveDirectory(Path relativeOrAbsolute)
            => directory.Resolve(relativeOrAbsolute).AsDirectory();

        public GetFileResult ResolveFile(Path relativeOrAbsolute)
            => directory.Resolve(relativeOrAbsolute).AsFile();

        public IEnumerable<RemoveSelfError> RemoveEntriesRecursive(UserContext context)
        {
            var entires = directory.EnumerateEntries().ToList();
            for (var i = entires.Count - 1; i >= 0; i--)
                if (entires[i].RemoveSelf(context) is { } error)
                    yield return error;
        }

        public IEnumerable<RemoveSelfError> RemoveSelfAndEntries(UserContext context)
        {
            var failed = false;
            foreach (var error in directory.RemoveEntriesRecursive(context))
            {
                yield return error;
                failed = true;
            }

            if (!failed && directory.RemoveSelf(context) is { } selfError)
                yield return selfError;
        }

    }

}
