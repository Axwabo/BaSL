namespace BaSL.SourceGenerators;

public interface IPositionalOption;

public abstract record Option(string Name);

public sealed record FlagOption(string Name, char Flag, bool Required, bool? DefaultValue) : Option(Name);

public sealed record PositionalOption(string Name, string Type, string? DefaultValue) : Option(Name), IPositionalOption;

public sealed record DirectoryOption(string Name, DefaultDirectory Default) : Option(Name), IPositionalOption;

public sealed record CancellationTokenOption(string Name) : Option(Name);

// TODO: support multiple collection types

public sealed record RestArgumentsOption(string Name) : Option(Name);

public enum DefaultDirectory
{

    None = -1,
    Current = 0,
    UserHome = 1

}
