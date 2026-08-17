using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FlagAttribute : Attribute
{

    // TODO: support multiple & strings
    public FlagAttribute(char value) => Value = value;

    public char Value { get; }

}
