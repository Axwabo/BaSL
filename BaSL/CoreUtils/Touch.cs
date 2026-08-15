using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

public sealed class Touch : App, IHelpProvider
{

    public Touch(ExecutableContext context) : base(context)
    {
    }

    public async Task DisplayHelpAsync(CancellationToken cancellationToken)
        => await StandardOutput.WriteLineAsync("Creates files without any content.", cancellationToken);

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var path = Path.ToPartialAbsolutePath(arg, WorkingDirectory);
            var directory = FileSystem.ResolveDirectory(path.Parent);
            if (!directory.Success)
            {
                await StandardError.WriteLineAsync(directory.Error.Message, cancellationToken);
                return 1;
            }

            var result = directory.Value.CreateFile(UserContext, arg);
            if (result.Success)
                continue;
            await StandardError.WriteLineAsync(result.Error.Message, cancellationToken);
            return 1;
        }

        return 0;
    }

}
