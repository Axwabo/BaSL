using BaSL.FileSystems;
using BaSL.Users;

namespace BaSL;

public sealed class OperatingSystem
{

    public OperatingSystem()
    {
        FileSystem = FileSystem.CreateVirtual();
        Root = new User("root", FileSystem.Root.CreateDirectory("root")) {IsSuperuser = true};
    }

    public FileSystem FileSystem { get; }

    internal User Root { get; }

}
