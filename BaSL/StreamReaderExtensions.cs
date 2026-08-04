#if NETSTANDARD
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS0169 // Field is never used
#pragma warning disable CS0414 // Field is assigned but its value is never used
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

// ReSharper disable once CheckNamespace
namespace System.IO;

public static class StreamReaderExtensions
{

    extension(StreamReader reader)
    {

        public ValueTask<string?> ReadLineAsync(CancellationToken token)
        {
            if (reader.GetType() != typeof(StreamReader))
                throw new NotSupportedException();
            var mock = Unsafe.As
        }

    }

}

file sealed class StreamReaderMock : TextReader
{

    private readonly byte[] _byteBuffer = null!;

    private readonly bool _closable;

    private readonly Stream _stream;

    // ReSharper disable once InconsistentNaming
    private bool _asyncIOInProgress;
    private Task _asyncReadTask = Task.CompletedTask;

    private int _byteLen;

    private int _bytePos;
    private char[] _charBuffer = null!;
    private int _charLen;
    private int _charPos;

    private bool _checkPreamble;
    private Decoder _decoder = null!;

    private bool _detectEncoding;

    private bool _disposed;
    private Encoding _encoding = null!;

    private bool _isBlocked;

    private int _maxCharsPerBuffer;

}
#endif
