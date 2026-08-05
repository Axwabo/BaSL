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

        // TODO: idk how to model this
        public static Modes DefaultFileModes => new(Mode.Rwx, 0, Mode.Read);

        public static Modes DefaultDirectoryModes => new(Mode.Rwx, 0, Mode.Rx);

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

        public CreateFileResult CreateFile(UserContext context, FileSystemEntryName name)
            => directory.CreateFile(context, name, Directory.DefaultFileModes);

        public CreateDirectoryResult CreateDirectory(UserContext context, FileSystemEntryName name)
            => directory.CreateDirectory(context, name, Directory.DefaultDirectoryModes);

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
            => directory.FileSystem.Resolve(relativeOrAbsolute.ToPartialAbsolute(directory.FullPath));

        public GetDirectoryResult ResolveDirectory(Path relativeOrAbsolute)
            => directory.Resolve(relativeOrAbsolute).AsDirectory();

        public GetFileResult ResolveFile(Path relativeOrAbsolute)
            => directory.Resolve(relativeOrAbsolute).AsFile();

        public Result<File, FileSystemError> ResolveFileOrCreate(UserContext context, Path relativeOrAbsolute)
        {
            var absolute = relativeOrAbsolute.ToPartialAbsolute(directory.FullPath);
            var existing = directory.ResolveFile(absolute);
            if (existing.Success)
                return existing.Value;
            var parent = directory.FileSystem.ResolveDirectory(absolute.Parent);
            if (!parent.Success)
                return parent.Error;
            var create = parent.Value.CreateFile(context, absolute.Name);
            return create.Success ? create.Value : create.Error;
        }

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

        public Result<Directory, Error> Mount(UserContext context, FileSystem fileSystem, FileSystemEntryName name)
        {
            if (directory is not IMountSupport mountSupport)
                return new MountError();
            var result = mountSupport.Mount(context, fileSystem, name);
            return result.Success ? result.Value : result.Error;
        }

    }

}

// TODO
file sealed record MountError() : Error("Directory does not support mounting");
