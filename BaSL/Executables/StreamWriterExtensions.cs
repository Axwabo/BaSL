using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BaSL.Executables;

public static class StreamWriterExtensions
{

    extension(StreamWriter writer)
    {

        public Task WriteAsync(string line, CancellationToken cancellationToken)
            => writer.WriteAsync(line.AsMemory(), cancellationToken);

        public Task WriteLineAsync(string line, CancellationToken cancellationToken)
            => writer.WriteLineAsync(line.AsMemory(), cancellationToken);

    }

}
