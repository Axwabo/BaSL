using BaSL.FileSystems;

namespace BaSL.Syntax;

public abstract record CommandLocation
{

    public static implicit operator CommandLocation(Path path) => new PathCommandLocation(path);

}

public sealed record NamedCommandLocation(string CommandName) : CommandLocation;

public sealed record PathCommandLocation(Path FullPath) : CommandLocation;
