using System.IO.Pipelines;

namespace BaSL.Executables.Pipes;

internal sealed class WriterStream : DelegatingStream
{

    private readonly PipeWrapper _wrapper;
    private bool _disposed;

    public WriterStream(PipeWrapper wrapper, Pipe pipe) : base(pipe.Writer.AsStream()) => _wrapper = wrapper;

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            _wrapper.Cancel();
        }

        base.Dispose(disposing);
    }

}
