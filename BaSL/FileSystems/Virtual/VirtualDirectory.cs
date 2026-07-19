using System.Collections.Generic;
using System.IO;
using BaSL.FileSystems.Mounted;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualDirectory : Directory, IMountSupport
{

    private readonly Dictionary<string, FileSystemEntry> _entries = [];

    private Mode _mode;

    public VirtualDirectory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Mode mode) : base(fileSystemAccess, parentDirectory, name) => _mode = mode;

    public override Mode Mode => _mode;

    public Directory Mount(FileSystem fileSystem, FileSystemEntryName name)
    {
        ThrowIfNoAccess();
        if (_entries.ContainsKey(name.Value))
            throw new IOException("Name conflict");
        var mount = new FileSystemMount(FileSystemAccess, FullPath, name, fileSystem);
        _entries[name.Value] = mount;
        return mount;
    }

    private void ThrowIfNoAccess()
    {
        if (!_mode.CanWrite)
            throw new IOException("Directory is immutable");
    }

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _entries.Values;

    public override Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        ThrowIfNoAccess();
        // TODO: allow files & folders with the same name?
        var directory = new VirtualDirectory(FileSystemAccess, FullPath, name, mode);
        _entries.Add(name.Value, directory);
        return directory;
    }

    public override File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        ThrowIfNoAccess();
        var file = new VirtualFile(FileSystemAccess, FullPath, name, mode);
        _entries.Add(name.Value, file);
        return file;
    }

    public override FileSystemEntry GetEntry(FileSystemEntryName name) => _entries[name.Value];

    public void MakeReadOnly() => _mode &= ~Mode.Write;

}
