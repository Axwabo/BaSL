using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public sealed class ExecutableContext
{

    internal static ExecutableContext Root(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError)
        => new(console, fileSystem, console.CurrentDirectory, args)
        {
            SourceInput = standardInput,
            SourceOutput = standardOutput,
            SourceError = standardError
        };

    internal static ExecutableContext Piped(ExecutableContext source, Console console, FileSystem fileSystem, ReadOnlyMemory<string> args) => new(console, fileSystem, console.CurrentDirectory, args)
    {
        SourceInput = source.StandardInput.Reader,
        SourceOutput = source.StandardOutput.Writer,
        SourceError = source.StandardError.Writer
    };

    private ExecutableContext(
        Console console,
        FileSystem fileSystem,
        Directory workingDirectory,
        ReadOnlyMemory<string> args
    )
    {
        StandardInput = new PipeWrapper();
        StandardOutput = new PipeWrapper();
        StandardError = new PipeWrapper();
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        SourceInput = StandardInput.Reader;
        SourceOutput = StandardOutput.Writer;
        SourceError = StandardError.Writer;
        DestinationInput = StandardInput.Writer;
        DestinationError = StandardOutput.Reader;
        DestinationOutput = StandardError.Reader;
    }

    private PipeWrapper StandardInput { get; }
    private PipeWrapper StandardOutput { get; }
    private PipeWrapper StandardError { get; }
    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader SourceInput { get; private init; }
    internal StreamWriter SourceOutput { get; private init; }
    internal StreamWriter SourceError { get; private init; }
    internal StreamWriter DestinationInput { get; }
    internal StreamReader DestinationOutput { get; }
    internal StreamReader DestinationError { get; }

    internal async Task CopyAsync() => await Task.WhenAll(
        SourceInput.BaseStream is ReaderStream ? SourceInput.BaseStream.CopyToAsync(DestinationOutput.BaseStream) : Task.CompletedTask,
        DestinationOutput.BaseStream.CopyToAsync(SourceOutput.BaseStream),
        DestinationError.BaseStream.CopyToAsync(SourceError.BaseStream)
    );

}
