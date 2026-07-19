namespace BaSL.FileSystems;

public abstract class FileSystemEntry
{

    private protected FileSystemEntry(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name)
    {
        FullPath = parentDirectory / name;
        FileSystemAccess = fileSystemAccess;
        Name = name;
    }

    protected internal FileSystemAccess FileSystemAccess { get; }

    public FileSystem FileSystem => FileSystemAccess.FileSystem;

    public Path FullPath { get; }

    public FileSystemEntryName Name { get; }

    public abstract Mode Mode { get; }

}
