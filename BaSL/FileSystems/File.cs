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

    private const string Shebang = "#!";

    private protected File(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, Inode inode) : base(fileSystemAccess, parentDirectory, name, inode)
    {
    }

    protected File(FileSystemAccess fileSystemAccess, Path parentDirectory, FileSystemEntryName name, User owner, Modes modes) : base(fileSystemAccess, parentDirectory, name, owner, modes)
    {
    }

    internal virtual Executable? Executable { get; set; }

    public abstract long SizeBytes { get; }

    public abstract OpenFileResult Open(UserContext context, OpenMode mode);

    public ExecuteFileResult Execute(ExecutableContext context, CancellationToken cancellationToken)
    {
        if (!Metadata.CanExecute(context.Shell.User))
            return OpenFileError.AccessDenied;
        if (Executable != null)
            return Process.Start(Executable, context, cancellationToken);
        var openResult = Open(context.Shell.UserContext, OpenMode.Read);
        if (!openResult.Success)
            return openResult.Error;
        string? line;
        using (var reader = new StreamReader(openResult.Value))
        {
            Span<char> span = stackalloc char[Shebang.Length];
            if (reader.Read(span) != Shebang.Length || span is not Shebang)
                return OpenFileError.NotExecutable;
            line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return OpenFileError.NotExecutable;
        }

        var interpreterStart = 0;
        for (; interpreterStart < line.Length; interpreterStart++)
            if (!char.IsWhiteSpace(line[interpreterStart]))
                break;
        if (interpreterStart >= line.Length)
            return OpenFileError.NotExecutable;
        var interpreterEnd = line.IndexOf(' ', interpreterStart);
        var path = new Path(interpreterEnd == -1 ? line[interpreterStart..] : line[interpreterStart..interpreterEnd]);
        var file = context.FileSystem.ResolveFile(path);
        if (!file.Success || file.Value.Executable is not {} executable)
            return OpenFileError.ShebangNotFound;
        var args = interpreterEnd == -1 ? context.Args : new Args([line[interpreterEnd..], ..context.Args]);
        return Process.Start(executable, ExecutableContext.Sub(context, context, context.FileSystem, args), );
    }

    public OpenFileError? MakeExecutable(UserContext context, Executable executable)
    {
        if (!Metadata.CanWrite(context))
            return OpenFileError.AccessDenied;
        Executable = executable;
        return null;
    }

}
