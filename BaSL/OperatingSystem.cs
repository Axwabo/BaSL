using BaSL.FileSystems;
using BaSL.Users;

namespace BaSL;

public sealed class OperatingSystem
{

    public OperatingSystem()
    {
        Root = new User("root") {IsSuperuser = true};
        FileSystem = FileSystem.CreateVirtual(Root);
    }

    internal User Root { get; }

    public FileSystem FileSystem { get; }

}
