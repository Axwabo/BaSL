using System;
using System.IO;
using System.Threading;
using BaSL.Executables;

namespace BaSL.FileSystems;

public abstract class File : FileSystemEntry
{

    private Executable? _executable;

    protected File(FileSystem fileSystem, Path parentDirectory, FileSystemEntryName name) : base(fileSystem, parentDirectory, name)
    {
    }

    public abstract long SizeBytes { get; }

    public abstract Stream Open();

    public Process Execute(ExecutableContext context, CancellationToken cancellationToken)
    {
        if (!Mode.CanExecute)
            throw new IOException("Access denied");
        if (_executable != null)
            return Process.Start(_executable, context, cancellationToken);
        using var reader = new StreamReader(Open());
        var line = reader.ReadLine();
        if (!line.AsSpan().StartsWith("#!"))
            throw new IOException("File is not executable");
        throw new NotImplementedException();
    }

    public void MakeExecutable(Executable executable)
    {
        if (!Mode.CanWrite)
            throw new IOException("Access denied");
        _executable = executable;
    }

}
