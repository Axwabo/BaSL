using System.Collections.Generic;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw);

    public abstract File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw);

    public abstract Directory GetDirectory(FileSystemEntryName name);

}
