using BaSL.Users;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public VirtualFileSystem(User owner) => Root = new VirtualDirectory(new FileSystemAccess(this), Path.Root, "", owner, new Modes(Mode.Rwx, Mode.Rwx, 0));

    public override Directory Root { get; }

}
