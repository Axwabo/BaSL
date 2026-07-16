using BaSL.FileSystems.Virtual;

namespace BaSL.FileSystems;

public abstract class FileSystem
{

    public static FileSystem CreateVirtual() => new VirtualFileSystem();

    public abstract Directory Root { get; }

    private protected FileSystem()
    {
    }

}
