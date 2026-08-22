using BaSL.Executables;
using BaSL.Executables.Attributes;

namespace BaSL.BuiltIns;

[Help("Sets a variable that is also available in subshells. Usage: export variable=value")]
internal sealed partial class Export : VariableCommand
{

    public Export(ExecutableContext context) : base(context)
    {
    }

    protected override void Process(string name, string value) => Exported[name] = Local[name] = value;

}
