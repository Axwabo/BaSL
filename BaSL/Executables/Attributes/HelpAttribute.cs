using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class HelpAttribute : Attribute
{

    public HelpAttribute(string help) => Help = help;

    public string Help { get; }

}
