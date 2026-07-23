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
        if (disposing && !_disposed)
        {
            _disposed = true;
            _wrapper.Cancel();
        }

        base.Dispose(disposing);
    }

    public override ValueTask DisposeAsync() => base.DisposeAsync();

}
