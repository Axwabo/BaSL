using System;
using System.Collections.Generic;
using System.IO;
using BaSL.Users;

namespace BaSL.FileSystems.Dev;

internal sealed class DevDirectory : Directory
{

    private static readonly Modes Modes = new(Mode.Read, 0, Mode.Read);

    private readonly Dictionary<string, FileSystemEntry> _files = new();

    public DevDirectory(DevFileSystem fileSystem, User owner) : base(new FileSystemAccess(fileSystem), Path.Root, "", new Inode(owner, Modes) {IsFrozen = true})
    {
        Add("null", Stream.Null);
        Add("zero", ZeroStream.Instance);
    }

    private void Add(string name, Stream stream) => _files.Add(name, new DevFile(this, name, stream));

    public override IEnumerable<FileSystemEntry> EnumerateEntries() => _files.Values;

    public override Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw) => throw new NotSupportedException();

    public override File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw) => throw new NotSupportedException();

    public override FileSystemEntry GetEntry(FileSystemEntryName name) => _files[name.Value];

}
