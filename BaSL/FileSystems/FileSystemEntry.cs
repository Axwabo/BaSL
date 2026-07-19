namespace BaSL.FileSystems;

public abstract class FileSystemEntry
{

    protected FileSystemEntry(FileSystem fileSystem, Path parentDirectory, FileSystemEntryName name)
    {
        FileSystem = fileSystem;
        FullPath = parentDirectory / name;
        Name = name;
    }

    public FileSystem FileSystem { get; internal set; }

    public Path FullPath { get; }

    public FileSystemEntryName Name { get; }

    public abstract Mode Mode { get; }

}
