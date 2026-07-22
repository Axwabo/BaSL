using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

internal sealed class PipeWrapper : IAsyncDisposable
{

    private readonly Pipe _pipe = new();

    public PipeWrapper()
    {
        Reader = new StreamReader(new ReaderStream(this, _pipe));
        Writer = new StreamWriter(new WriterStream(this, _pipe)) {AutoFlush = true};
    }

    internal CancellationTokenSource Cts { get; } = new();

    public StreamReader Reader { get; }

    public StreamWriter Writer { get; }

    public async ValueTask DisposeAsync()
    {
        Cts.Dispose();
        Reader.Dispose();
        await Writer.DisposeAsync();
    }

}
