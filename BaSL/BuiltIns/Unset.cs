using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

[Help("Unsets the given variables.")]
internal sealed partial class Unset : SyncCommand
{

    public Unset(ExecutableContext context) : base(context)
    {
    }

    protected override void Execute()
    {
        foreach (var arg in Args)
        {
            Local.Remove(arg);
            Exported.Remove(arg);
        }
    }

}
