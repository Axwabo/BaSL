using System;
using System.IO;
using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemExtensions
{

    extension(FileSystem fileSystem)
    {

        public GetEntryResult Resolve(Path path)
        {
            var links = 0;
            return fileSystem.ResolveInternal(path, ref links);
        }

        // TODO: stack
        private GetEntryResult ResolveInternal(Path path, ref int links)
        {
            if (path.IsEmpty)
                return GetEntryError.NotFound;
            FileSystemEntry entry = fileSystem.Root;
            foreach (var s in path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries))
            {
                if (s is FileSystemEntryName.Current)
                    continue;
                if (entry is SymbolicLink link)
                {
                    if (links++ > 10)
                        return GetEntryError.SymlinkLimit;
                    var followResult = fileSystem.ResolveInternal(link.Target, ref links);
                    if (!followResult.Success)
                        return followResult.Error;
                    entry = followResult.Value; // TODO: nested symlinks?
                }

                if (entry is not Directory directory)
                    break;
                var result = s is FileSystemEntryName.Parent ? directory.GetParent().AsEntry() : directory.GetEntry(s);
                if (!result.Success)
                    return result.Error;
                entry = result.Value;
            }

            return entry;
        }

        public GetDirectoryResult ResolveDirectory(Path path) => fileSystem.Resolve(path).AsDirectory();

        public GetFileResult ResolveFile(Path path) => fileSystem.Resolve(path).AsFile();

        public Result<Stream, FileSystemError> OpenFile(Path path, UserContext context, OpenMode mode)
        {
            var file = fileSystem.ResolveFile(path);
            if (!file.Success)
                return file.Error;
            var open = file.Value.Open(context, mode);
            return open.Success ? open.Value : open.Error;
        }

    }

}
