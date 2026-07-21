using System;
using System.IO;
using System.Threading;
using BaSL.Executables;
using BaSL.FileSystems.Extensions;
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

    public abstract Stream Open(UserContext context, OpenMode mode);

    public Process Execute(ExecutableContext context, CancellationToken cancellationToken)
    {
        if (!Metadata.CanExecute(context.Console.User))
            throw new IOException("Access denied");
        if (Executable != null)
            return Process.Start(Executable, context, cancellationToken);
        using var reader = new StreamReader(Open(context.Console.UserContext, OpenMode.Read));
        var line = reader.ReadLine();
        if (!line.AsSpan().StartsWith("#!"))
            throw new IOException("File is not executable");
        throw new NotImplementedException();
    }

    public void MakeExecutable(UserContext context, Executable executable)
    {
        if (!Metadata.CanWrite(context))
            throw new IOException("Access denied");
        Executable = executable;
    }

}
