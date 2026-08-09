using BaSL.Users;

namespace BaSL.FileSystems;

public sealed class SymbolicLink : FileSystemEntry
{

    internal SymbolicLink(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode inode, Path target)
        : base(fileSystemAccess, parentDirectory, name, inode)
        => Target = target;

    internal SymbolicLink(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes, Path target)
        : base(fileSystemAccess, parentDirectory, name, owner, modes)
        => Target = target;

    public Path Target { get; }

}
