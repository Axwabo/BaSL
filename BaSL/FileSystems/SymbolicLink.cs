using BaSL.Users;

namespace BaSL.FileSystems;

public sealed class SymbolicLink : FileSystemEntry
{

    internal SymbolicLink(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode inode) : base(fileSystemAccess, parentDirectory, name, inode)
    {
    }

    internal SymbolicLink(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    public required Path TargetPath { get; init; }

}
