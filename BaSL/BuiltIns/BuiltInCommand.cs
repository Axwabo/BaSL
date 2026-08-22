using System.IO;
using BaSL.Executables;

namespace BaSL.BuiltIns;

internal abstract partial class BuiltInCommand : App
{

    public required Variables Variables { get; init; }

    public required Variables Exported { get; init; }

    protected new StreamWriter StandardError => Context.IsRoot ? StandardOutput : base.StandardError;

}
