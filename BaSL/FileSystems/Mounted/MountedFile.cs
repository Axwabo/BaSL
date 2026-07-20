using System;
using System.IO;
using BaSL.Executables;
using BaSL.Users;

namespace BaSL.FileSystems.Mounted;

internal sealed class MountedFile : File
{

    public static MountedFile Create(MountedDirectory directory, File original)
        => original is MountedFile
            ? throw new ArgumentException("Cannot double-mount a file")
            : new MountedFile(directory.FileSystemAccess, directory.FullPath, original);

    private readonly File _original;

    private MountedFile(FileSystemAccess fileSystemAccess, Path parentDirectory, File original)
        : base(fileSystemAccess, parentDirectory, original.Name, original.Metadata)
        => _original = original;

    public override long SizeBytes => _original.SizeBytes;

    internal override Executable? Executable
    {
        get => _original.Executable;
        set => _original.Executable = value;
    }

    public override Stream Open(UserContext context) => _original.Open(context);

}
