using System.IO;
using BaSL.Executables;

namespace BaSL.FileSystems;

public abstract class File : FileSystemEntry
{

    private Executable? _executable;

    public abstract Stream Open();

    public Process Execute(ExecutableContext context)
        => !Mode.CanExecute
            ? throw new IOException("Access denied")
            : _executable == null
                ? throw new IOException("File is not an executable")
                : new Process(context, _executable);

    public void MakeExecutable(Executable executable)
    {
        if (!Mode.CanWrite)
            throw new IOException("Access denied");
        _executable = executable;
    }

}
