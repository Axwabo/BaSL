using BaSL.Executables;

namespace BaSL.BuiltIns;

internal sealed class DefaultArgumentParsers
{

    static DefaultArgumentParsers()
    {
        ArgumentParser<bool>.Delegate = bool.TryParse;
        ArgumentParser<float>.Delegate = float.TryParse;
        ArgumentParser<double>.Delegate = double.TryParse;
        ArgumentParser<int>.Delegate = int.TryParse;
        ArgumentParser<byte>.Delegate = byte.TryParse;
    }

}
