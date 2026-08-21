using System.Diagnostics.CodeAnalysis;

namespace BaSL.Executables;

public static class ArgumentParser<T>
{

    public delegate bool TryParseDelegate(string argument, [NotNullWhen(true)] out T? result);

    public static TryParseDelegate? Delegate;

    public static bool TryParse(string argument, [NotNullWhen(true)] out T? result)
    {
        if (Delegate != null)
            return Delegate(argument, out result);
        result = default;
        return false;
    }

}
