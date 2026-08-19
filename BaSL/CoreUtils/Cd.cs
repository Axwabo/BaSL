using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.FileSystems.Extensions;

namespace BaSL.CoreUtils;

[Help("""
      Changes the current directory.
      If no argument is provided, changes to the current user's home directory.
      """)]
public sealed partial class Cd : App
{

    public Cd(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = WorkingDirectory.ResolveDirectory(Args.FirstOrDefault(UserContext.User.Home));
        if (!result.Success)
        {
            await StandardOutput.WriteLineAsync(result.Error.Message, cancellationToken);
            return 1;
        }

        Shell.CurrentDirectory = result.Value;
        return 0;
    }

}
