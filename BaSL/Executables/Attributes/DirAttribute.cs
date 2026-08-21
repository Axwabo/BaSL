using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class DirAttribute : Attribute
{

    public DirAttribute(bool defaultToWorkingDirectory = false) => DefaultToWorkingDirectory = defaultToWorkingDirectory;

    public bool DefaultToWorkingDirectory { get; }

}
