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

    public int Length => Value.Length;

    public string this[int index] => Value.Span[index];

    public string this[Index index] => Value.Span[index];

    public Args this[Range range] => Value[range];

    public bool Equals(Args? other) => other.HasValue && (other.GetValueOrDefault().IsEmpty && Value.IsEmpty || other.Value.Value.Span.SequenceEqual(Value.Span));

    public override string ToString()
    {
        if (Length == 0)
            return "";
        var length = Value.Length - 1;
        foreach (var arg in this)
            length += arg.Length;
        Span<char> buffer = stackalloc char[length];
        buffer.Fill(' ');
        var i = 0;
        foreach (var arg in this)
        {
            arg.AsSpan().CopyTo(buffer[i..]);
            i += arg.Length + 1;
        }

        return buffer.ToString();
    }

}
