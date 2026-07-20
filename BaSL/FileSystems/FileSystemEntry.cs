using BaSL.Users;

namespace BaSL.FileSystems;

public abstract class FileSystemEntry
{

    private protected FileSystemEntry(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode inode)
    {
        FullPath = parentDirectory / name;
        FileSystemAccess = fileSystemAccess;
        Name = name;
        Metadata = inode;
    }

    private protected FileSystemEntry(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : this(fileSystemAccess, parentDirectory, name, new Inode(owner, modes))
    {
    }

    protected internal FileSystemAccess FileSystemAccess { get; }

    public FileSystem FileSystem => FileSystemAccess.FileSystem;

    public Path FullPath { get; }

    public FileSystemEntryName Name { get; }

    public virtual Inode Metadata { get; }

    public abstract Mode Mode { get; }

}
