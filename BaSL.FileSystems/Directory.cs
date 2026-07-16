using System.Collections.Generic;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract Directory CreateDirectory(FileSystemEntryName name, Permissions permissions = Permissions.Rw);

    public abstract File CreateFile(FileSystemEntryName name, Permissions permissions = Permissions.Rw);

    public abstract Directory GetDirectory(FileSystemEntryName name);

}
