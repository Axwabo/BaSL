namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public override Directory Root { get; }

    public override Directory Home { get; }

    public VirtualFileSystem()
    {
        var root = new VirtualDirectory("/");
        Root = root;
        Home = Root.CreateDirectory("home").CreateDirectory("user");
        root.MakeReadOnly();
    }

}
