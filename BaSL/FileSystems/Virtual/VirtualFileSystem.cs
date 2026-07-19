namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFileSystem : FileSystem
{

    public VirtualFileSystem()
    {
        var root = new VirtualDirectory(new FileSystemAccess(this), Path.Root, "", Mode.Rwx);
        Root = root;
        Home = Root.CreateDirectory("home").CreateDirectory("user");
        root.CreateDirectory("usr").CreateDirectory("bin");
        root.MakeReadOnly();
    }

    public override Directory Root { get; }

    public Directory Home { get; }

}
