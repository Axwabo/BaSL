using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public abstract class ExecutableContext
{

    internal static ExecutableContext Root(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamWriter standardOutput, StreamWriter standardError)
    {
        var inputPipe = new PipeWrapper();
        return new RootExecutableContext(console, fileSystem, console.CurrentDirectory, args, inputPipe, standardOutput, standardError);
    }

    internal static ExecutableContext Piped(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args) => new(console, fileSystem, console.CurrentDirectory, args)
    {
        Parent = source
    };

    internal static ExecutableContext Sunken(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamWriter standardOutput, StreamWriter standardError)
        => new(console, fileSystem, console.CurrentDirectory, args)
        {
            SourceOutput = standardOutput,
            SourceError = standardError,
            DisposeOutput = true
        };

    private static async Task CopyAsync(StreamReader source, StreamWriter destination, PipeWrapper cancellation, bool dispose = false)
    {
        try
        {
            await source.BaseStream.CopyToAsync(destination.BaseStream, cancellation.CancellationToken);
        }
        catch (InvalidOperationException)
        {
            // "Reading is not allowed after reader was completed." NERD EMOJI
        }
        finally
        {
            if (dispose)
                await destination.DisposeAsync();
        }
    }

    private protected ExecutableContext(
        Console console,
        FileSystem fileSystem,
        Directory workingDirectory,
        ReadOnlyMemory<string> args,
        StreamReader sourceInput,
        StreamWriter sourceOutput,
        StreamWriter sourceError
    )
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        SourceInput = sourceInput;
        SourceOutput = sourceOutput;
        SourceError = sourceError;
    }

    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader SourceInput { get; }
    internal StreamWriter SourceOutput { get; }
    internal StreamWriter SourceError { get; }
    internal abstract StreamWriter DestinationInput { get; }
    internal abstract StreamReader DestinationOutput { get; }
    internal abstract StreamReader DestinationError { get; }

    internal abstract Task CopyAsync();

    internal abstract ValueTask DisposeAsync();

}
