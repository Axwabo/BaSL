using System.IO;
using BaSL.Executables;

namespace BaSL.BuiltIns;

internal abstract partial class BuiltInCommand : App
{

    public required (Variables Local, Variables Exported) Vars
    {
        init => (Local, Exported) = value;
    }

    protected Variables Local { get; private set; }

    protected Variables Exported { get; private set; }

    protected new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

}
