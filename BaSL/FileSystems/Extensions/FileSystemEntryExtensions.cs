using BaSL.FileSystems.Errors;

namespace BaSL.FileSystems.Extensions;

public static class FileSystemEntryExtensions
{

    extension(FileSystemEntry entry)
    {

        public GetDirectoryResult GetParent()
        {
            var parent = entry.FullPath.Parent;
            return parent.IsEmpty
                ? GetEntryError.NotFound
                : entry.FileSystem.ResolveDirectory(parent);
        }

    }

}
