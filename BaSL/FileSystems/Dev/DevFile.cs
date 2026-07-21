using System.IO;
using BaSL.Executables;
using BaSL.Users;

namespace BaSL.FileSystems.Dev;

internal sealed class DevFile : File
{

    private static readonly Modes Modes = new(Mode.Rw, 0, Mode.Rw);

    private readonly Stream _stream;

    public DevFile(DevDirectory directory, FileSystemEntryName name, Stream stream)
        : base(directory.FileSystemAccess, directory.FullPath, name, new Inode(directory.Metadata.Owner, Modes))
        => _stream = stream;

    public override long SizeBytes => long.MaxValue;

    internal override Executable? Executable
    {
        get => null;
        set { }
    }

    public override OpenFileResult Open(UserContext context, OpenMode mode) => _stream;

}
