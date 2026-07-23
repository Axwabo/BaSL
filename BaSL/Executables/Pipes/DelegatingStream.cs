using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

internal abstract class DelegatingStream : Stream
{

    private readonly Stream _stream;

    protected DelegatingStream(Stream stream) => _stream = stream;

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _stream.BeginRead(buffer, offset, count, callback, state);

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => _stream.BeginWrite(buffer, offset, count, callback, state);

    public override void CopyTo(Stream destination, int bufferSize) => _stream.CopyTo(destination, bufferSize);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => _stream.CopyToAsync(destination, bufferSize, cancellationToken);

    public override int EndRead(IAsyncResult asyncResult) => _stream.EndRead(asyncResult);

    public override void EndWrite(IAsyncResult asyncResult) => _stream.EndWrite(asyncResult);

    public override void Flush() => _stream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    public override int Read(Span<byte> buffer) => _stream.Read(buffer);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _stream.ReadAsync(buffer, cancellationToken);

    public override int ReadByte() => _stream.ReadByte();

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

    public override void Write(ReadOnlySpan<byte> buffer) => _stream.Write(buffer);

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _stream.WriteAsync(buffer, cancellationToken);

    public override void WriteByte(byte value) => _stream.WriteByte(value);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _stream.Dispose();
    }

    public override ValueTask DisposeAsync() => _stream.DisposeAsync();

}
