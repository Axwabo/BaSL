using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemExtensions
{

    extension(FileSystem fileSystem)
    {

        public GetEntryResult Resolve(Path path, bool followLinks = true)
        {
            var links = 0;
            return fileSystem.ResolveInternal(path, followLinks, ref links);
        }

        // TODO: stack
        private GetEntryResult ResolveInternal(Path path, bool followLinks, ref int links)
        {
            if (path.IsEmpty)
                return GetEntryError.NotFound;
            FileSystemEntry entry = fileSystem.Root;
            foreach (var s in path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries))
            {
                if (s is FileSystemEntryName.Current)
                    continue;
                if (followLinks && !fileSystem.FollowLink(ref links, ref entry, out var resolveInternal))
                    return resolveInternal;
                if (entry is not Directory directory)
                    break;
                var result = s is FileSystemEntryName.Parent ? directory.GetParent().AsEntry() : directory.GetEntry(s);
                if (!result.Success)
                    return result.Error;
                entry = result.Value;
            }

            return !followLinks || fileSystem.FollowLink(ref links, ref entry, out var finalInternal)
                ? entry
                : finalInternal;
        }

        private bool FollowLink(ref int links, ref FileSystemEntry entry, [NotNullWhen(false)] out GetEntryError? error)
        {
            while (entry is SymbolicLink link)
            {
                if (links++ > 10)
                {
                    error = GetEntryError.SymlinkLimit;
                    return false;
                }

                var followResult = fileSystem.ResolveInternal(link.Target, true, ref links);
                if (!followResult.Success)
                {
                    error = followResult.Error;
                    return false;
                }

                entry = followResult.Value;
            }

            error = null;
            return true;
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
