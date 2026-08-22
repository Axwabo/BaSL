using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.Terminal;

[Help("Clears the screen.")]
public sealed partial class Clear : App
{

    public override Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        System.Console.Clear();
        return Task.FromResult(0);
    }

}
