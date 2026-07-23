using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

[DebuggerDisplay("Writer of {Id}")]
internal sealed class WriterStream : DelegatingStream
{

    private readonly PipeWrapper _wrapper;
    private bool _disposed;

    public WriterStream(PipeWrapper wrapper, Pipe pipe) : base(pipe.Writer.AsStream()) => _wrapper = wrapper;

    private int Id => _wrapper.GetHashCode();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Cancel();
        base.Dispose(disposing);
    }

    private void Cancel()
    {
        if (_disposed)
            return;
        _disposed = true;
        _wrapper.Cancel();
    }

    public override ValueTask DisposeAsync()
    {
        Cancel();
        return base.DisposeAsync();
    }

}
