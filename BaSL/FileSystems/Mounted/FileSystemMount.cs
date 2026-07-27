using System.Collections.Generic;
using BaSL.FileSystems.Errors;
using BaSL.Users;

namespace BaSL.FileSystems.Mounted;

internal sealed class FileSystemMount : Directory
{

    private readonly Directory _root;

    public FileSystemMount(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, FileSystem fileSystemToMount)
        : base(fileSystemAccess, parentDirectory, name, fileSystemToMount.Root.Metadata)
    {
        var mounted = new MountedFileSystem(fileSystemToMount, fileSystemAccess.FileSystem, FullPath);
        _root = mounted.Root;
    }

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _root.EnumerateEntries();

    public override CreateDirectoryResult CreateDirectory(UserContext context, FileSystemEntryName name, Modes modes) => _root.CreateDirectory(context, name, modes);

    public override CreateFileResult CreateFile(UserContext context, FileSystemEntryName name, Modes modes) => _root.CreateFile(context, name, modes);

    public override GetEntryResult GetEntry(FileSystemEntryName name) => _root.GetEntry(name);

    public override RemoveChildError? RemoveEntry(UserContext context, FileSystemEntryName name) => _root.RemoveEntry(context, name);

    public override GetDirectoryResult GetDirectory(FileSystemEntryName name) => _root.GetDirectory(name);

    public override GetFileResult GetFile(FileSystemEntryName name) => _root.GetFile(name);

}
