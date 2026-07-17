using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BaSL.Shell;

public sealed class Shell : Program
{

    public override async Task<int> ExecuteAsync()
    {
        var bin = FileSystem.Root.GetDirectory("usr").GetDirectory("bin");
        var echo = bin.CreateExecutable("echo", (system, stream, arg3, arg4, arg5) => new Echo
        {
            FileSystem = system,
            Arguments = arg5,
            StandardInput = stream,
            StandardOutput = arg3,
            StandardError = arg4
        }.ExecuteAsync());
        var line = await new StreamReader(StandardInput).ReadLineAsync();
        var args = line.Split().Select(e => e.AsMemory()).ToArray();
        await echo.ExecuteAsync(FileSystem, StandardInput, StandardOutput, StandardError, args);
        return 0;
    }

}
