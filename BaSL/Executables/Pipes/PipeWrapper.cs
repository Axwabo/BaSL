using System.IO;
using System.IO.Pipelines;
using System.Threading;

namespace BaSL.Executables.Pipes;

internal sealed class PipeWrapper
{

    private readonly Pipe _pipe = new();

    public PipeWrapper()
    {
        Reader = new ReaderStream(this, _pipe);
        Writer = new WriterStream(this, _pipe);
    }

    internal CancellationTokenSource Cts { get; } = new();

    public Stream Reader { get; }

    public Stream Writer { get; }

}
