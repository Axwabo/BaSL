using BaSL.FileSystems.Virtual;
using BaSL.Users;

namespace BaSL.FileSystems;

public abstract class FileSystem
{

    public static FileSystem CreateVirtual(User owner) => new VirtualFileSystem(owner);

    private protected FileSystem()
    {
    }

    public abstract Directory Root { get; }

}
