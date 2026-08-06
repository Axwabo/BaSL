using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;

namespace BaSL.CoreUtils;

public sealed class Env : App
{

    public Env(ExecutableContext context) : base(context)
    {
    }

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length != 0)
            throw new NotImplementedException();
        foreach (var kvp in UserContext.User.Environment)
        {
            await StandardOutput.WriteAsync(kvp.Key, cancellationToken);
            await StandardOutput.WriteAsync('=');
            await StandardOutput.WriteLineAsync(kvp.Value, cancellationToken);
        }

        return 0;
    }

}
