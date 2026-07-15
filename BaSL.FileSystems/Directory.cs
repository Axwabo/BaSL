using System.Collections.Generic;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

}
