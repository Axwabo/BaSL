using System.Runtime.CompilerServices;
using System.Text;

namespace Experiments;

public class StreamReaderMock : TextReader
{

    public static void AmongUssy()
    {
        var streamReader = new StreamReader(Stream.Null);
        var buffer = Unsafe.As<StreamReader, StreamReaderMock>(ref streamReader)._charBuffer;
        Console.WriteLine(buffer);
    }

    private readonly byte[] _byteBuffer = null!;

    private readonly bool _closable;

    private readonly Stream _stream;
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
