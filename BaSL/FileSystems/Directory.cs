using System.Collections.Generic;
using BaSL.Users;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    private protected Directory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode node) : base(fileSystemAccess, parentDirectory, name, node)
    {
    }

    protected Directory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw);

    public abstract File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw);

    public abstract FileSystemEntry GetEntry(FileSystemEntryName name);

    public virtual Directory GetDirectory(FileSystemEntryName name) => (Directory) GetEntry(name);

    public virtual File GetFile(FileSystemEntryName name) => (File) GetEntry(name);

}
