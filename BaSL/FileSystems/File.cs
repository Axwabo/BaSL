using System;
using System.IO;
using System.Threading;
using BaSL.Executables;
using BaSL.Users;

namespace BaSL.FileSystems;

public abstract class File : FileSystemEntry
{

    private protected File(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode inode) : base(fileSystemAccess, parentDirectory, name, inode)
    {
    }

    protected File(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    internal virtual Executable? Executable { get; set; }

    public abstract long SizeBytes { get; }

    public abstract Stream Open();

    public Process Execute(ExecutableContext context, CancellationToken cancellationToken)
    {
        if (!Mode.CanExecute)
            throw new IOException("Access denied");
        if (Executable != null)
            return Process.Start(Executable, context, cancellationToken);
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
        Executable = executable;
    }

}
