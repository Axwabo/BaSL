using System.Collections.Generic;
using System.Linq;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public static class DirectoryExtensions
{

    extension(FileSystemEntry entry)
    {

        public RemoveError? Remove()
        {
            var parent = entry.GetParent();
            if (!parent.Success)
                return new ParentDirectoryNotFound(entry.FullPath, parent.Error);
            var remove = parent.Value.RemoveEntry(entry.Name);
            return remove is not null
                ? new CannotRemove(entry.FullPath, remove)
                : null;
        }

    }

    extension(Directory directory)
    {

        public IEnumerable<RemoveError> RemoveEntriesRecursive()
        {
            var entires = directory.EnumerateEntries().ToList();
            for (var i = entires.Count - 1; i >= 0; i--)
                if (entires[i].Remove() is { } error)
                    yield return error;
        }

        public IEnumerable<RemoveError> RemoveSelfAndEntries()
        {
            var failed = false;
            foreach (var error in directory.RemoveEntriesRecursive())
            {
                yield return error;
                failed = true;
            }

            if (!failed && directory.Remove() is { } selfError)
                yield return selfError;
        }

    }

}
