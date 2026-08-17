using System;

namespace BaSL.Executables.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ExecuteAttribute : Attribute
{

    static ExecuteAttribute()
    {
        ArgumentParser<bool>.Delegate = bool.TryParse;
        ArgumentParser<float>.Delegate = float.TryParse;
        ArgumentParser<double>.Delegate = double.TryParse;
        ArgumentParser<int>.Delegate = int.TryParse;
        ArgumentParser<byte>.Delegate = byte.TryParse;
    }

}
