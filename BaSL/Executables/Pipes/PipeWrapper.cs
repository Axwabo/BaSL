using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

internal sealed class PipeWrapper
{

    private readonly Pipe _pipe = new();

    public PipeWrapper()
    {
        Reader = new ReaderStream(this, _pipe);
        Writer = new WriterStream(this, _pipe);
    }

    public bool Completed { get; internal set; }

    public Stream Reader { get; }

    public Stream Writer { get; }

}

file sealed class ReaderStream : Stream
{

    private readonly Stream _stream;

    private readonly PipeWrapper _wrapper;

    public ReaderStream(PipeWrapper wrapper, Pipe pipe)
    {
        _wrapper = wrapper;
        _stream = pipe.Reader.AsStream();
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

    public override int Read(byte[] buffer, int offset, int count) => _wrapper.Completed ? 0 : _stream.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _stream.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

}

file sealed class WriterStream : Stream
{

    private readonly Stream _stream;

    private readonly PipeWrapper _wrapper;

    public WriterStream(PipeWrapper wrapper, Pipe pipe)
    {
        _wrapper = wrapper;
        _stream = pipe.Writer.AsStream();
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

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.WriteAsync(buffer, offset, count, cancellationToken);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _stream.WriteAsync(buffer, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        _wrapper.Completed = true;
        base.Dispose(disposing);
    }

}
