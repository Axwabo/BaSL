using System.Collections.Generic;
using System.IO;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualDirectory : Directory
{

    private readonly Dictionary<string, FileSystemEntry> _entries = [];

    private Permissions _permissions;

    private void ThrowIfNoAccess()
    {
        if (!_permissions.CanWrite)
            throw new IOException("Directory is immutable");
    }

    public VirtualDirectory(Path fullPath, Permissions permissions = Permissions.Rw)
    {
        FullPath = fullPath;
        _permissions = permissions;
    }

    public override Path FullPath { get; }

    public override Permissions Permissions => _permissions;

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _entries.Values;

    public override Directory CreateDirectory(FileSystemEntryName name, Permissions permissions = Permissions.Rw)
    {
        ThrowIfNoAccess();
        // TODO: allow files & folders with the same name?
        var directory = new VirtualDirectory(FullPath / name, permissions);
        _entries.Add(name.Value, directory);
        return directory;
    }

    public override File CreateFile(FileSystemEntryName name, Permissions permissions = Permissions.Rw)
    {
        ThrowIfNoAccess();
        var file = new VirtualFile(FullPath / name, permissions);
        _entries.Add(name.Value, file);
        return file;
    }

    public void MakeReadOnly() => _permissions &= ~Permissions.Write;

}
