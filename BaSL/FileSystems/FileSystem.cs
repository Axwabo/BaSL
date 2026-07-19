using BaSL.FileSystems.Virtual;

namespace BaSL.FileSystems;

public abstract class FileSystem
{

    private protected FileSystem()
    {
    }

    public abstract Directory Root { get; }

    public static FileSystem CreateVirtual() => new VirtualFileSystem();

}
