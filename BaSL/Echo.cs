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
        var args = Args;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            StandardOutput.Write(arg);
            if (i != args.Length - 1)
                StandardOutput.Write(' ');
        }

        return Task.FromResult(0);
    }

}
