using System;
using System.Collections.Generic;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract Directory CreateDirectory(FileSystemEntryName name);

    public abstract File CreateFile(FileSystemEntryName name);

}
