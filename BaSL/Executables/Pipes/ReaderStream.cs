using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables.Pipes;

internal sealed class ReaderStream : DelegatingStream
{

    private readonly PipeWrapper _wrapper;

    public ReaderStream(PipeWrapper wrapper, Pipe pipe) : base(pipe.Reader.AsStream()) => _wrapper = wrapper;

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _wrapper.CancellationToken);
        var token = cts.Token;
        try
        {
            await base.CopyToAsync(destination, bufferSize, token);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

}
