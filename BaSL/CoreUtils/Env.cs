using System;
using System.Threading;
using System.Threading.Tasks;
using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.CoreUtils;

[Help("""
      Prints the shell's exported variables.
      Launching executables is not yet supported.
      """)]
public sealed partial class Env : App
{

    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Args.Length != 0)
            throw new NotImplementedException();
        foreach (var kvp in Environment)
        {
            await StandardOutput.WriteAsync(kvp.Key, cancellationToken);
            await StandardOutput.WriteAsync('=');
            await StandardOutput.WriteLineAsync(kvp.Value, cancellationToken);
        }

        return 0;
    }

}
