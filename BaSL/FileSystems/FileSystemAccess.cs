namespace BaSL.FileSystems;

public sealed class FileSystemAccess
{

    internal FileSystemAccess(FileSystem fileSystem) => FileSystem = fileSystem;

    public FileSystem FileSystem { get; internal set; }

}
