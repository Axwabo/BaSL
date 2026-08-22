using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems.Errors;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

[Help("Removes an empty directory.")]
public sealed partial class Rmdir : App
{

    public Rmdir(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length == 0)
            return await ErrorAsync("Directory must be specified", cancellationToken);
        var entry = WorkingDirectory.ResolveDirectory(Args[0]);
        return !entry.Success
            ? await ErrorAsync(RemoveChildError.NothingToRemove, cancellationToken)
            : WorkingDirectory.FullPath.Value.StartsWith(entry.Value.FullPath.Value)
                ? await ErrorAsync("Refusing to remove current directory (or parent)", cancellationToken)
                : entry.Value.RemoveSelf(UserContext) is { } error
                    ? await ErrorAsync(error, cancellationToken)
                    : 0;
    }

}
