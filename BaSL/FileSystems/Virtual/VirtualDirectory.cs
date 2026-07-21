using System.Collections.Generic;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.FileSystems.Mounted;
using BaSL.Users;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualDirectory : Directory, IMountSupport
{

    private readonly Dictionary<string, FileSystemEntry> _entries = [];

    public VirtualDirectory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    public CreateDirectoryResult Mount(FileSystem fileSystem, FileSystemEntryName name, User owner, Modes modes)
    {
        if (!Metadata.CanWrite(owner))
            return CreateEntryError.AccessDenied;
        if (_entries.ContainsKey(name.Value))
            return CreateEntryError.NameCollision;
        var mount = new FileSystemMount(FileSystemAccess, FullPath, name, fileSystem);
        _entries[name.Value] = mount;
        return mount;
    }

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _entries.Values;

    public override CreateDirectoryResult CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        if (!Metadata.OwnerMode.CanWrite)
            return CreateEntryError.AccessDenied;
        // TODO: allow files & folders with the same name?
        var directory = new VirtualDirectory(FileSystemAccess, FullPath, name, Metadata.Owner, Metadata.Modes with {Owner = mode});
        _entries.Add(name.Value, directory);
        return directory;
    }

    public override CreateFileResult CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        if (!Metadata.OwnerMode.CanWrite)
            return CreateEntryError.AccessDenied;
        var file = new VirtualFile(FileSystemAccess, FullPath, name, Metadata.Owner, Metadata.Modes with {Owner = mode});
        _entries.Add(name.Value, file);
        return file;
    }

    public override FileSystemEntry GetEntry(FileSystemEntryName name) => _entries[name.Value];

}
