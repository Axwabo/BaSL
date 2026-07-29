using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

internal sealed class RootExecutableContext : ExecutableContext
{

    private readonly PipeWrapper _inputPipe;

    public RootExecutableContext(Console console, FileSystem fileSystem, Directory currentDirectory, ReadOnlyMemory<string> args, PipeWrapper inputPipe, StreamWriter standardOutput, StreamWriter standardError)
        : base(console, fileSystem, currentDirectory, args, inputPipe.Reader, standardOutput, standardError)
        => _inputPipe = inputPipe;

    internal override StreamWriter DestinationInput => null!;
    internal override StreamReader DestinationOutput => null!;
    internal override StreamReader DestinationError => null!;
    internal override bool IsRoot { get; }
    internal override Task CopyAsync() => throw new NotImplementedException();

    internal override ValueTask DisposeAsync() => throw new NotImplementedException();

}
