using System;

namespace BaSL.FileSystems;

public readonly record struct Path(string Value)
{

    public static Path Combine(Path left, Path right)
    {
        // TODO: validation and whatnot ughh
        if (left.Value.EndsWith("/")||right.Value.StartsWith("/"))
            return left.Value + right.Value;
        var length = left.Value.Length;
        Span<char> span = stackalloc char[length + right.Value.Length + 1];
        span[length] = '/';
        left.Value.AsSpan().CopyTo(span);
        right.Value.AsSpan().CopyTo(span[(length + 1)..]);
        return span.ToString();
    }

    public static implicit operator Path(string value) => new(value);

    public static Path operator /(Path left, Path right) => Combine(left, right);

}
