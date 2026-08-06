using System;
using System.IO;
using System.Threading.Tasks;
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
            return new VirtualFileStream(this, _data, _length, mode == OpenMode.ReadWrite);
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

    internal void Release()
    {
        lock (_access)
        {
            _used = false;
        }
    }

}

// TODO: async-only maybe?
file sealed class VirtualFileStream : Stream
{

    private readonly VirtualFile _file;

    private readonly MemoryStream _stream;

    private bool _disposed;

    public VirtualFileStream(VirtualFile file, byte[] data, int length, bool canWrite)
    {
        _file = file;
        if (canWrite)
        {
            _stream = new MemoryStream(length);
            _stream.Write(data.AsSpan(0, length));
        }
        else
            _stream = new MemoryStream(data, 0, length, false);
    }

    public override bool CanRead => ThrowIfDisposed(_stream.CanRead);

    public override bool CanSeek => ThrowIfDisposed(_stream.CanSeek);

    public override bool CanWrite => ThrowIfDisposed(_stream.CanWrite);

    public override long Length => ThrowIfDisposed(_stream.Length);

    public override long Position
    {
        get => _stream.Position;
        set
        {
            ThrowIfDisposed();
            _stream.Position = value;
        }
    }

    public override void Flush()
    {
        ThrowIfDisposed();
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        ThrowIfDisposed();
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        ThrowIfDisposed();
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ThrowIfDisposed();
        _stream.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        GC.SuppressFinalize(this);
        if (disposing)
        {
            if (CanWrite)
                _file.Release(_stream.GetBuffer(), (int) _stream.Length);
            else
                _file.Release();
        }

        _stream.Dispose();
        _disposed = true;
    }

    public override ValueTask DisposeAsync()
    {
        Dispose(true);
        return default;
    }

    ~VirtualFileStream() => Dispose(false);

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException("VirtualFileStream");
    }

    private T ThrowIfDisposed<T>(T returnValue)
    {
        ThrowIfDisposed();
        return returnValue;
    }

}
