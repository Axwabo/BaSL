using System;
using BaSL.FileSystems;

namespace BaSL.Syntax;

public abstract record CommandLocation
{

    public static implicit operator CommandLocation(string auto) => new AutoCommandLocation(auto);

    public static implicit operator CommandLocation(Path path) => new PathCommandLocation(path);

    public static implicit operator ReadOnlyMemory<char>(CommandLocation location) => location switch
    {
        AutoCommandLocation auto => auto.Phrase.AsMemory(),
        PathCommandLocation path => path.FullPath.Value.AsMemory(),
        _ => ReadOnlyMemory<char>.Empty
    };

}

/// <summary>
/// Defines a command that may be a built-in command, an executable in PATH, or an absolute or relative file path.
/// </summary>
/// <param name="Phrase">The abstract phrase.</param>
public sealed record AutoCommandLocation(string Phrase) : CommandLocation;

/// <summary>
/// Defines a command based on the executable's fullly qualified path in the file system.
/// </summary>
/// <param name="FullPath">The path to the executable.</param>
public sealed record PathCommandLocation(Path FullPath) : CommandLocation;
