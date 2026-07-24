using System;
using System.Collections.Generic;
using System.IO;
using BaSL.FileSystems.Errors;

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

    public override CreateDirectoryResult CreateDirectory(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        var result = _original.CreateDirectory(name, mode);
        return result.Success ? (Directory) Cache(result.Value) : result;
    }

    public override CreateFileResult CreateFile(FileSystemEntryName name, Mode mode = Mode.Rw)
    {
        var result = _original.CreateFile(name, mode);
        return result.Success ? (File) Cache(result.Value) : result;
    }

    public override GetEntryResult GetEntry(FileSystemEntryName name)
    {
        if (_cachedEntries.TryGetValue(name.Value, out var cached))
            return cached;
        var result = _original.GetEntry(name);
        return result.Success ? Cache(result.Value) : result;
    }

    public override RemoveEntryError? RemoveEntry(FileSystemEntryName name)
    {
        var error = _original.RemoveEntry(name);
        if (error is null)
            _cachedEntries.Remove(name.Value);
        return error;
    }

}
