using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

[DebuggerDisplay("Reader of {Id}")]
internal sealed class ReaderStream : DelegatingStream
{

    private readonly PipeWrapper _wrapper;

    public ReaderStream(PipeWrapper wrapper, Pipe pipe) : base(pipe.Reader.AsStream()) => _wrapper = wrapper;
    private int Id => _wrapper.GetHashCode();

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _wrapper.CancellationToken);
        var token = cts.Token;
        token.Register(() => System.Console.WriteLine("CANCELED"));
        try
        {
            await base.CopyToAsync(destination, bufferSize, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

}
