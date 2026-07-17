namespace BaSL.FileSystems;

public abstract class FileSystemEntry
{

    public abstract Path FullPath { get; }

    public abstract Mode Mode { get; }

}
