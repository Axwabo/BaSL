using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;

namespace BaSL;

public sealed class Console
{

    public required FileSystem FileSystem { get; init; }
    public required StreamReader StandardInput { get; init; }
    public required StreamWriter StandardOutput { get; init; }
    public required StreamWriter StandardError { get; init; }

    public async Task<int> ExecuteAsync()
    {
        var bin = FileSystem.Root.GetDirectory("usr").GetDirectory("bin");
        var echo = bin.CreateFile("echo", Mode.Rwx);
        echo.MakeExecutable(context => new Echo(context).ExecuteAsync());
        var line = await StandardInput.ReadLineAsync();
        var args = line.Split();
        var stdin = new Pipe();
        var stdout = new Pipe();
        var stderr = new Pipe();
        var context = new ExecutableContext(FileSystem, stdin.Reader, stdout.Writer, stderr.Writer, args);
        var process = echo.Execute(context);
        var copyStdout = stdout.Reader.CopyToAsync(StandardOutput.BaseStream);
        var copyStdin = stderr.Reader.CopyToAsync(StandardError.BaseStream);
        await process.WaitForExitAsync();
        await Task.WhenAll(
            stdout.Writer.CompleteAsync().AsTask(),
            stderr.Writer.CompleteAsync().AsTask(),
            copyStdin,
            copyStdout
        );
        return 0;
    }

}
