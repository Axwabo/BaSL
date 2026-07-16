using System.IO;
using System.Threading.Tasks;

namespace BaSL.Shell;

public sealed class Echo : Program
{

    public override Task<int> ExecuteAsync()
    {
        var writer = new StreamWriter(StandardOutput);
        var args = Arguments.Span;
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            writer.Write(arg.Span);
            if (i != args.Length - 1)
                writer.Write(' ');
        }

        return Task.FromResult(0);
    }

}
