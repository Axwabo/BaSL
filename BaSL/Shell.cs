using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;

namespace BaSL;

public sealed class Shell : Program
{

    public Shell(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync()
    {
        var bin = FileSystem.Root.GetDirectory("usr").GetDirectory("bin");
        var echo = bin.CreateFile("echo", Mode.Rwx);
        echo.MakeExecutable(context => new Echo(context).ExecuteAsync());
        var line = await StandardInput.ReadLineAsync();
        var args = line.Split();
        var context = new ExecutableContext(FileSystem, StandardInput, StandardOutput, StandardError, args);
        var process = echo.Execute(context);
        await process.WaitForExitAsync();
        return 0;
    }

}
