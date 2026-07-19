using System;

namespace BaSL.FileSystems.Mounted;

internal sealed class MountedFileSystem : FileSystem
{

    internal MountedFileSystem(Directory root, Path mountPoint)
    {
        if (root.FileSystemAccess.FileSystem is MountedFileSystem)
            throw new ArgumentException("Cannot mount an already-mounted file system");
        root.FileSystemAccess.FileSystem = this;
        Root = MountedDirectory.Create(this, mountPoint, root);
    }

    public override Directory Root { get; }

}
