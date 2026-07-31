using System;

namespace BaSL.Syntax;

public readonly record struct Args(ReadOnlyMemory<string> Value)
{

    public static implicit operator Args(ReadOnlyMemory<string> args) => new(args);

    public static implicit operator Args(string[] args) => new(args);

    public static implicit operator StandaloneStatement?(Args args) => StandaloneStatement.FromArgs(args.Value);

    public Args(params string[] args) : this(args.AsMemory())
    {
    }

}
