namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public VirtualFileSystem() => Root = new VirtualDirectory(new FileSystemAccess(this), Path.Root, "", Mode.Rwx);

    public override Directory Root { get; }

}
