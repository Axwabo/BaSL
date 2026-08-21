using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

[Help("Creates files without any content.")]
public sealed partial class Touch : App
{

    public Touch(ExecutableContext context) : base(context)
    {
    }

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
