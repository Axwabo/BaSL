using System;

namespace BaSL;

public readonly record struct Args(ReadOnlyMemory<string> Value)
{

    public static implicit operator Args(Memory<string> args) => new(args);

    public static implicit operator Args(ReadOnlyMemory<string> args) => new(args);

    public static implicit operator Args(string[] args) => new(args);

    public Args(params string[] args) : this(args.AsMemory())
    {
    }

    public bool IsEmpty => Value.IsEmpty;

    public int Length => Value.Length;

    public string this[int index] => Value.Span[index];

    public Args this[Range range] => Value[range];

}
