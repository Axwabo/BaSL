using System.Collections.Generic;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    protected Directory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name) : base(fileSystemAccess, parentDirectory, name)
    {
    }

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw);

    public abstract File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw);

    public abstract FileSystemEntry GetEntry(FileSystemEntryName name);

    public virtual Directory GetDirectory(FileSystemEntryName name) => (Directory) GetEntry(name);

    public virtual File GetFile(FileSystemEntryName name) => (File) GetEntry(name);

}
