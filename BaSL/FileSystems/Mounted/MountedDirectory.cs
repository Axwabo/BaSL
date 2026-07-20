using System;
using System.Collections.Generic;
using System.IO;

namespace BaSL.FileSystems.Mounted;

internal sealed class MountedDirectory : Directory
{

    public static MountedDirectory Create(MountedFileSystem mountedFileSystem, Path mountPoint, Directory original)
        => original is MountedDirectory
            ? throw new ArgumentException("Cannot double-mount a directory")
            : new MountedDirectory(new FileSystemAccess(mountedFileSystem), mountPoint, original);

    private readonly Dictionary<string, FileSystemEntry> _cachedEntries = [];

    private readonly Directory _original;

    private MountedDirectory(MountedDirectory parent, Directory original) : this(parent.FileSystemAccess, parent.FullPath, original)
    {
    }

    private MountedDirectory(FileSystemAccess fileSystemAccess, Path parentDirectory, Directory original)
        : base(fileSystemAccess, parentDirectory, original.Name, original.Metadata)
        => _original = original;

    public override Mode Mode => _original.Mode;

    public override IEnumerable<FileSystemEntry> EnumerateEntries()
    {
        foreach (var entry in _original.EnumerateEntries())
        {
            var name = entry.Name.Value;
            if (_cachedEntries.TryGetValue(name, out var cached))
                yield return cached;
            else
                yield return Cache(entry);
        }
    }

    private FileSystemEntry Cache(FileSystemEntry entry) => _cachedEntries[entry.Name.Value] = entry switch
    {
        Directory directory => new MountedDirectory(this, directory),
        File file => MountedFile.Create(this, file),
        _ => throw new IOException($"Invalid filesystem entry {entry}")
    };

    public override Directory CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw)
        => (Directory) Cache(_original.CreateDirectory(name, mode));

    public override File CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw)
        => (File) Cache(_original.CreateFile(name, mode));

    public override FileSystemEntry GetEntry(FileSystemEntryName name)
        => _cachedEntries.TryGetValue(name.Value, out var cached)
            ? cached
            : Cache(_original.GetEntry(name));

}
