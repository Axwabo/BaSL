using System.Collections.Generic;

namespace BaSL.FileSystems.Mounted;

internal sealed class FileSystemMount : Directory
{

    private readonly Directory _root;

    public FileSystemMount(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, FileSystem fileSystemToMount)
        : base(fileSystemAccess, parentDirectory, name)
    {
        var mounted = new MountedFileSystem(fileSystemToMount, FullPath);
        _root = mounted.Root;
    }

    public override Mode Mode => _root.Mode;

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _root.EnumerateEntries();

    public override Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw) => _root.CreateDirectory(name, mode);

    public override File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw) => _root.CreateFile(name, mode);

    public override FileSystemEntry GetEntry(FileSystemEntryName name) => _root.GetEntry(name);

    public override Directory GetDirectory(FileSystemEntryName name) => _root.GetDirectory(name);

    public override File GetFile(FileSystemEntryName name) => _root.GetFile(name);

}
