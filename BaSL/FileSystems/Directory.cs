using System.Collections.Generic;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Users;

namespace BaSL.FileSystems;

public abstract class Directory : FileSystemEntry
{

    private protected Directory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode node) : base(fileSystemAccess, parentDirectory, name, node)
    {
    }

    protected Directory(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    public abstract IEnumerable<FileSystemEntry> EnumerateEntries();

    public abstract CreateDirectoryResult CreateDirectory(UserContext context, FileSystemEntryName name, Modes modes);

    public abstract CreateFileResult CreateFile(UserContext context, FileSystemEntryName name, Modes modes);

    public abstract GetEntryResult GetEntry(FileSystemEntryName name);

    public virtual GetDirectoryResult GetDirectory(FileSystemEntryName name) => GetEntry(name).AsDirectory();

    public virtual GetFileResult GetFile(FileSystemEntryName name) => GetEntry(name).AsFile();

    public abstract RemoveChildError? RemoveEntry(UserContext context, FileSystemEntryName name);

}
