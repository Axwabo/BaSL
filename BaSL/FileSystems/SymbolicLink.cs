namespace BaSL.FileSystems;

public sealed class SymbolicLink : FileSystemEntry
{

    internal SymbolicLink(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode inode, Path target)
        : base(fileSystemAccess, parentDirectory, name, inode)
        => Target = target;

    public Path Target { get; }

}
