using System.Collections.Generic;
using BaSL.FileSystems.Errors;
using BaSL.Users;

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

        public RemoveSelfError? RemoveSelf(UserContext context)
        {
            var parent = entry.GetParent();
            if (!parent.Success)
                return new ParentDirectoryNotFound(entry.FullPath, parent.Error);
            var remove = parent.Value.RemoveEntry(context, entry.Name);
            return remove is not null
                ? new CannotRemoveSelf(entry.FullPath, remove)
                : null;
        }

        public IEnumerable<RemoveSelfError> Remove(UserContext context, bool recurse, bool continueOnError)
        {
            if (entry is not Directory directory)
            {
                if (entry.RemoveSelf(context) is { } error)
                    yield return error;
                yield break;
            }

            if (!recurse)
            {
                yield return new CannotRemoveDirectory(directory.FullPath);
                yield break;
            }

            foreach (var other in directory.RemoveSelfAndEntries(context))
            {
                yield return other;
                if (!continueOnError)
                    yield break;
            }
        }

    }

}
