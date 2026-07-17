using System.IO;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.FileSystems;

public abstract class File : FileSystemEntry
{

    private Executable? _executable;

    public abstract Stream Open();

    public Task<int> ExecuteAsync(ExecutableContext context)
        => !Mode.CanExecute
            ? throw new IOException("Access denied")
            : _executable == null
                ? throw new IOException("File is not an executable")
                : _executable(context);

    public void MakeExecutable(Executable executable)
    {
        if (!Mode.CanWrite)
            throw new IOException("Access denied");
        _executable = executable;
    }

}
