namespace BaSL.SourceGenerators;

public abstract record Option(string Name);

public sealed record PositionalOption(string Name, string Type, string? DefaultValue) : Option(Name);

public sealed record FlagOption(string Name, char Flag, bool Required, bool? DefaultValue) : Option(Name);
