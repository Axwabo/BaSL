namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public override Directory Root { get; }

    public Directory Home { get; }

    public VirtualFileSystem()
    {
        var root = new VirtualDirectory("/", Permissions.Read);
        Root = root;
        Home = Root.CreateDirectory("home", Permissions.Read).CreateDirectory("user");
        root.MakeReadOnly();
    }

}
