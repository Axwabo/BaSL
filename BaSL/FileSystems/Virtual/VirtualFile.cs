using System;
using System.IO;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;
using BaSL.Users;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFile : File
{

    private readonly object _access = new();

    private byte[] _data = [];
    private int _length;
    private bool _used;

    public VirtualFile(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    public override long SizeBytes => _length;

    public override OpenFileResult Open(UserContext context, OpenMode mode)
    {
        if (!Metadata.CanRead(context) || mode == OpenMode.ReadWrite && !Metadata.CanWrite(context))
            return OpenFileError.AccessDenied;
        lock (_access)
        {
            if (_used)
                return OpenFileError.AccessViolation;
            _used = true;
            return new VirtualFileStream(this, _data, _length);
        }
    }

    internal void Release(byte[] buffer, int length)
    {
        lock (_access)
        {
            _used = false;
            _data = buffer;
            _length = length;
        }
    }

}

// TODO: async-only maybe?
file sealed class VirtualFileStream : Stream
{

    private readonly VirtualFile _file;

    private readonly MemoryStream _stream;

    public VirtualFileStream(VirtualFile file, byte[] data, int length)
    {
        _file = file;
        _stream = new MemoryStream();
        _stream.Write(data.AsSpan(0, length));
        _stream.Position = 0;
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public override void Flush() => _stream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

    protected override void Dispose(bool disposing)
    {
        GC.SuppressFinalize(this);
        if (disposing) // TODO: ???
            _file.Release(_stream.GetBuffer(), (int) _stream.Length);
        _stream.Dispose();
    }

    ~VirtualFileStream() => Dispose(false);

}
