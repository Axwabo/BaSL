using System;

namespace BaSL.FileSystems.Mounted;

internal sealed class MountedFileSystem : FileSystem
{

    internal MountedFileSystem(FileSystem original, FileSystem target, Path mountPoint)
    {
        if (original is MountedFileSystem)
            throw new ArgumentException("Cannot mount an already-mounted file system");
        original.Root.FileSystemAccess.FileSystem = target;
        Root = MountedDirectory.Create(target, mountPoint, original.Root);
    }

    public override Directory Root { get; }

}
