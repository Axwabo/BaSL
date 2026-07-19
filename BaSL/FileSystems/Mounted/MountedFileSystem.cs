namespace BaSL.FileSystems.Mounted;

public sealed class MountedFileSystem : FileSystem
{

    internal MountedFileSystem(Directory root, Path mountPoint)
    {
        root.FileSystemAccess.FileSystem = this;
        Root = new MountedDirectory(new FileSystemAccess(this), mountPoint, root);
    }

    public override Directory Root { get; }

}
