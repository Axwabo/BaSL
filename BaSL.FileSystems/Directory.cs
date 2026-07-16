using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract Directory CreateDirectory(FileSystemEntryName name, Permissions permissions = Permissions.Rw);

    public abstract File CreateFile(FileSystemEntryName name, Permissions permissions = Permissions.Rw);

    public abstract IExecutable CreateExecutable(FileSystemEntryName name, Func<FileSystem, Stream, Stream, Stream, ReadOnlyMemory<ReadOnlyMemory<char>>, Task<int>> execute, Permissions permissions = Permissions.Rwx);

    public abstract Directory GetDirectory(FileSystemEntryName name);

}
