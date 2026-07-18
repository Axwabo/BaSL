using BaSL.Executables;

namespace BaSL.Shell.Console;

public sealed class Pwd : Executables.Program
{

    public Pwd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var writer = StandardOutput;
        await writer.WriteLineAsync(Console.CurrentDirectory.FullPath.Value);
        return 0;
    }

}
