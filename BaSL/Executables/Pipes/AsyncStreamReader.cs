using System;
using System.IO;

namespace BaSL.Executables.Pipes;

public sealed class AsyncStreamReader : StreamReader
{

    public AsyncStreamReader(Stream stream) : base(stream)
    {
    }

    [Obsolete("Synchronous reading is not supported", true)]
    public override int Read(char[] buffer, int index, int count) => throw new NotSupportedException();

    [Obsolete("Synchronous reading is not supported", true)]
    public override int Read(Span<char> buffer) => throw new NotSupportedException();

    [Obsolete("Synchronous reading is not supported", true)]
    public override int ReadBlock(char[] buffer, int index, int count) => throw new NotSupportedException();

    [Obsolete("Synchronous reading is not supported", true)]
    public override int ReadBlock(Span<char> buffer) => throw new NotSupportedException();

    [Obsolete("Synchronous reading is not supported", true)]
    public override string? ReadLine() => throw new NotSupportedException();

    [Obsolete("Synchronous reading is not supported", true)]
    public override string ReadToEnd() => throw new NotSupportedException();

}
