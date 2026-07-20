using System.IO;
using BaSL.Executables;

namespace BaSL.FileSystems.Dev;

internal sealed class DevFile : File
{

    private readonly Stream _stream;

    public DevFile(DevDirectory directory, FileSystemEntryName name, Stream stream)
        : base(directory.FileSystemAccess, directory.FullPath, name, directory.Metadata)
        => _stream = stream;

    public override Mode Mode => Mode.Rw;

    public override long SizeBytes => long.MaxValue;

    internal override Executable? Executable
    {
        get => null;
        set { }
    }

    public override Stream Open() => _stream;

}
