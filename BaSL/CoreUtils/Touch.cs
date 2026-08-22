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

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var arg in Args)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var path = Path.ToPartialAbsolutePath(arg, WorkingDirectory);
            var directory = FileSystem.ResolveDirectory(path.Parent);
            if (!directory.Success)
                return await ErrorAsync(directory.Error, cancellationToken);
            var result = directory.Value.CreateFile(UserContext, arg);
            if (!result.Success)
                return await ErrorAsync(result.Error, cancellationToken);
        }

        return 0;
    }

}
