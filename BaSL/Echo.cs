using System.IO;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL;

public sealed class Echo : Program
{

    public Echo(ExecutableContext context) : base(context)
    {
    }

    public override Task<int> ExecuteAsync()
    {
        using var writer = new StreamWriter(StandardOutput.AsStream());
        var args = Args;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            writer.Write(arg);
            if (i != args.Length - 1)
                writer.Write(' ');
        }

        return Task.FromResult(0);
    }

}
