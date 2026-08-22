using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;
using BaSL.Syntax;
using BaSL.Users;

namespace BaSL.CoreUtils;

[Help("Runs the given command as the root user.")]
public sealed partial class Sudo : App
{

    // TODO: no freaking clue on how to solve auth
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        => Args.IsEmpty
            ? await ErrorAsync("Args required", cancellationToken)
            : await new Process(
                BaShell.CreateSubshell(
                    Context,
                    StandaloneStatement.FromArgs(Args),
                    new UserContext(Context.Console.OperatingSystem.Root)
                ), cancellationToken
            ).WaitForExitAsync();

}
