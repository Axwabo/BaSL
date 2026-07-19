using System;
using System.IO;

namespace BaSL.FileSystems.Dev;

public sealed class ZeroStream : Stream
{

    public static readonly ZeroStream Instance = new();

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => long.MaxValue;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        Array.Fill(buffer, (byte) 0, offset, count);
        return count;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
    }

}
