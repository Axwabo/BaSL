using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

[Help("""
      "Dumps the contents of the specified file(s) to stdout, each followed by a new line."
      "If no argument is given, writes stdin lines."
      """)]
public sealed partial class Cat : App
{

    public Cat(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            return await ReadStandardInputAsync(cancellationToken);
        foreach (var arg in Args)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var entry = WorkingDirectory.ResolveFile(arg);
            if (!entry.Success)
            {
                await StandardError.WriteLineAsync(entry.Error.Message, cancellationToken);
                return 1;
            }

            var open = entry.Value.Open(UserContext, OpenMode.Read);
            if (!open.Success)
            {
                await StandardError.WriteLineAsync(open.Error.Message, cancellationToken);
                return 1;
            }

            await using var stream = open.Value;
            await stream.CopyToAsync(StandardOutput.BaseStream, cancellationToken);
        }

        return 0;
    }

    private async Task<int> ReadStandardInputAsync(CancellationToken cancellationToken)
    {
        // TODO: why is the stream not ending??
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await StandardInput.ReadLineAsync();
            if (line == null)
                break;
            await StandardOutput.WriteLineAsync(line, cancellationToken);
        }

        return 0;
    }

}
