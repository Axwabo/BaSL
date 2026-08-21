using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class DefaultToAttribute : Attribute
{

    public DefaultToAttribute(DefaultDirectory defaultDirectory) => DefaultDirectory = defaultDirectory;

    public DefaultDirectory DefaultDirectory { get; }

}

public enum DefaultDirectory
{

    Current = 0,
    UserHome = 1

}
