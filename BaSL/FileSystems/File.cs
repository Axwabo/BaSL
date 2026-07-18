using System.IO;
using System.Threading;
using BaSL.Executables;

namespace BaSL.FileSystems;

public abstract class File : FileSystemEntry
{

    private Executable? _executable;

    public abstract Stream Open();

    public Process Execute(ExecutableContext context, CancellationToken cancellationToken)
        => !Mode.CanExecute
            ? throw new IOException("Access denied")
            : _executable == null
                ? throw new IOException("File is not an executable")
                : Process.Start(_executable, context, cancellationToken);

    public void MakeExecutable(Executable executable)
    {
        if (!Mode.CanWrite)
            throw new IOException("Access denied");
        _executable = executable;
    }

}
