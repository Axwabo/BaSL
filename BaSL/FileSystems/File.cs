using System;
using System.IO;
using System.Threading;
using BaSL.Executables;
using BaSL.FileSystems.Errors;
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

    public Result<Process, OpenFileError> Execute(ExecutableContext context, CancellationToken cancellationToken)
    {
        if (!Metadata.CanExecute(context.Console.User))
            return OpenFileError.AccessDenied;
        if (Executable != null)
            return Process.Start(Executable, context, cancellationToken);
        using var reader = new StreamReader(Open(context.Console.UserContext, OpenMode.Read));
        var line = reader.ReadLine();
        // TODO
        if (!line.AsSpan().StartsWith("#!"))
            throw new IOException("File is not executable");
        throw new NotImplementedException();
    }

    public OpenFileError? MakeExecutable(UserContext context, Executable executable)
    {
        if (!Metadata.CanWrite(context))
            return OpenFileError.AccessDenied;
        Executable = executable;
        return null;
    }

}
