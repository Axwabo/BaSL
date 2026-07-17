using System.IO;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL;

public sealed class Pwd : Program
{

    public Pwd(ExecutableContext context) : base(context)
    {
    }

    public override Task<int> ExecuteAsync()
    {
        using var writer = new StreamWriter(StandardOutput.AsStream());
        writer.WriteLine(Console.CurrentDirectory.FullPath.Value);
        return Task.FromResult(0);
    }

}
