using System.Collections.Generic;

namespace BaSL.FileSystems.Virtual;

internal class VirtualDirectory : Directory
{

    public VirtualDirectory(Path fullPath) => FullPath = fullPath;

    private readonly Dictionary<string, FileSystemEntry> _entries = [];

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _entries.Values;

    public override Directory CreateDirectory(FileSystemEntryName name)
    {
        // TODO: allow files & folders with the same name?
        var directory = new VirtualDirectory(FullPath / name);
        _entries.Add(name.Value, directory);
        return directory;
    }

    public override File CreateFile(FileSystemEntryName name)
    {
        var file = new VirtualFile(FullPath / name);
        _entries.Add(name.Value, file);
        return file;
    }

    public override Path FullPath { get; }

}
