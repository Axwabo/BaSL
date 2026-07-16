namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public override Directory Root { get; }

    public Directory Home { get; }

    public VirtualFileSystem()
    {
        Root = new VirtualDirectory("/");
        Home = Root.CreateDirectory("home").CreateDirectory("user");
    }

}
