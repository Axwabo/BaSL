using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

internal sealed class PipeWrapper : IAsyncDisposable
{

    private readonly CancellationTokenSource _cts = new();

    private readonly Pipe _pipe = new();
    private bool _canceled;

    public PipeWrapper()
    {
        Reader = new StreamReader(_pipe.Reader.AsStream());
        Writer = new StreamWriter(_pipe.Writer.AsStream()) {AutoFlush = true};
    }

    public CancellationToken CancellationToken => _cts.Token;

    public StreamReader Reader { get; }

    public StreamWriter Writer { get; }

    public async ValueTask DisposeAsync()
    {
        Cancel();
        Reader.Dispose();
        await Writer.DisposeAsync();
    }

    public void Cancel()
    {
        if (_canceled)
            return;
        _canceled = true;
        _cts.Cancel();
        _cts.Dispose();
    }

}
