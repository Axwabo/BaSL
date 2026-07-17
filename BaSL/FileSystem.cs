using BaSL.FileSystems.Virtual;

namespace BaSL.FileSystems;

public abstract class FileSystem
{

    public static FileSystem CreateVirtual() => new VirtualFileSystem();

    public abstract Directory Root { get; }
    public abstract Directory Home { get; }

    private protected FileSystem()
    {
    }

}
