namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public override Directory Root { get; } = new VirtualDirectory("/");

}
