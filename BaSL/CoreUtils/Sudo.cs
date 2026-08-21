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

    public Sudo(ExecutableContext context) : base(context)
    {
    }

    // TODO: no freaking clue on how to solve auth
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!Args.IsEmpty)
            return await new Process(BaShell.CreateSubshell(Context, StandaloneStatement.FromArgs(Args), new UserContext(Context.Console.OperatingSystem.Root)), cancellationToken).WaitForExitAsync();
        await StandardError.WriteLineAsync("Args required", cancellationToken);
        return 1;
    }

}
