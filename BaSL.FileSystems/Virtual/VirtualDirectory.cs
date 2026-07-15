using System.Collections.Generic;

namespace BaSL.FileSystems.Virtual;

internal class VirtualDirectory : Directory
{

    private readonly Dictionary<string, FileSystemEntry> _entries = [];

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _entries.Values;

    public override Path FullPath { get; }

}
