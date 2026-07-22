using System;
using System.IO;
using System.Threading.Tasks;
using BaSL.Executables.Pipes;
using BaSL.FileSystems;
using Directory = BaSL.FileSystems.Directory;

namespace BaSL.Executables;

public class ExecutableContext
{

    internal static ExecutableContext Root(Console console, FileSystem fileSystem, ReadOnlyMemory<string> args, StreamReader standardInput, StreamWriter standardOutput, StreamWriter standardError)
    {
        var inPipe = new PipeWrapper();
        var outPipe = new PipeWrapper();
        var errPipe = new PipeWrapper();
        return new ExecutableContext(
            inPipe,
            outPipe,
            errPipe,
            console,
            fileSystem,
            console.CurrentDirectory,
            args,
            standardInput,
            inPipe.Reader,
            outPipe.Writer,
            standardOutput,
            outPipe.Writer,
            standardError
        );
    }

    private protected ExecutableContext(
        PipeWrapper standardInput,
        PipeWrapper standardOutput,
        PipeWrapper standardError,
        Console console,
        FileSystem fileSystem,
        Directory workingDirectory,
        ReadOnlyMemory<string> args,
        StreamReader consumerInputReader,
        StreamWriter producerInput,
        StreamReader producerOutput,
        StreamWriter consumerOutputWriter,
        StreamReader producerError,
        StreamWriter consumerErrorWriter
    )
    {
        Console = console;
        FileSystem = fileSystem;
        WorkingDirectory = workingDirectory;
        Args = args;
        ConsumerInput = consumerInputReader;
        ConsumerOutput = consumerOutputWriter;
        ConsumerError = consumerErrorWriter;
        StandardInput = standardInput;
        ProducerInput = producerInput;
        ProducerError = producerError;
        ProducerOutput = producerOutput;
    }

    private protected PipeWrapper? StandardInput { get; }
    internal Console Console { get; }
    internal FileSystem FileSystem { get; }
    internal Directory WorkingDirectory { get; }
    internal ReadOnlyMemory<string> Args { get; }
    internal StreamReader ConsumerInput { get; }
    internal StreamWriter ConsumerOutput { get; }
    internal StreamWriter ConsumerError { get; }
    internal StreamWriter ProducerInput { get; }
    internal StreamReader ProducerOutput { get; }
    internal StreamReader ProducerError { get; }

    internal async ValueTask DisposeAsync()
    {
        ConsumerInput.Dispose();
        await ConsumerOutput.DisposeAsync();
        await ConsumerError.DisposeAsync();
        await ProducerInput.DisposeAsync();
        ProducerOutput.Dispose();
        ProducerError.Dispose();
    }

}
