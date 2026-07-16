namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public override Directory Root { get; }

    public override Directory Home { get; }

    public VirtualFileSystem()
    {
        var root = new VirtualDirectory("/");
        var usr = root.CreateDirectory("usr");
        var bin = root.CreateDirectory("bin");
        Root = root;
        Home = Root.CreateDirectory("home").CreateDirectory("user");
        ((VirtualDirectory) bin).MakeReadOnly();
        ((VirtualDirectory) usr).MakeReadOnly();
        root.MakeReadOnly();
    }

}
