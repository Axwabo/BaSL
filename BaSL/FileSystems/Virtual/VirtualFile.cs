using System;
using System.IO;

namespace BaSL.FileSystems.Virtual;

internal sealed class VirtualFile : File
{

    private readonly object _access = new();

    private byte[] _data = [];
    private int _length;
    private bool _used;

    public VirtualFile(FileSystem fileSystem, Path parentDirectory, FileSystemEntryName name, Mode mode) : base(fileSystem, parentDirectory, name) => Mode = mode;

    public override Mode Mode { get; }

    public override long SizeBytes => _length;

    public override Stream Open()
    {
        if (!Mode.CanRead)
            throw new IOException("File is not readable");
        lock (_access)
        {
            if (_used)
                throw new IOException("Access violation");
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
        _stream = new MemoryStream(data, 0, length, true, true);
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
        _stream.Dispose();
        if (disposing) // TODO: ???
            _file.Release(_stream.GetBuffer(), (int) _stream.Length);
    }

    ~VirtualFileStream() => Dispose(false);

}
