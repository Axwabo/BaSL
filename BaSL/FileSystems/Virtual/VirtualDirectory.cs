using System.Collections.Generic;
using System.IO;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualDirectory : Directory
{

    private readonly Dictionary<string, FileSystemEntry> _entries = [];

    private Mode _mode;

    private void ThrowIfNoAccess()
    {
        if (!_mode.CanWrite)
            throw new IOException("Directory is immutable");
    }

    public VirtualDirectory(Path fullPath, Mode mode = Mode.Rw)
    {
        FullPath = fullPath;
        _mode = mode;
    }

    public override Path FullPath { get; }

    public override Mode Mode => _mode;

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _entries.Values;

    public override Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        ThrowIfNoAccess();
        // TODO: allow files & folders with the same name?
        var directory = new VirtualDirectory(FullPath / name, mode);
        _entries.Add(name.Value, directory);
        return directory;
    }

    public override File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        ThrowIfNoAccess();
        var file = new VirtualFile(FullPath / name, mode);
        _entries.Add(name.Value, file);
        return file;
    }

    public override FileSystemEntry GetEntry(FileSystemEntryName name) => _entries[name.Value];

    public void MakeReadOnly() => _mode &= ~Mode.Write;

}
