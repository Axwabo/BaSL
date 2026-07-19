namespace BaSL.FileSystems.Dev;

public sealed class DevFileSystem : FileSystem
{

    public DevFileSystem() => Root = new DevDirectory(this);

    public override Directory Root { get; }

}
