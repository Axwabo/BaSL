using BaSL.Users;

namespace BaSL.FileSystems.Dev;

public sealed class DevFileSystem : FileSystem
{

    public DevFileSystem(User owner) => Root = new DevDirectory(this, owner);

    public override Directory Root { get; }

}
