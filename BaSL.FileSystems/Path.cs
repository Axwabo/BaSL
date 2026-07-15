using System;

namespace BaSL.FileSystems;

public readonly record struct Path(string Value)
{

    public static Path Combine(Path left, Path right)
    {
        // TODO: validation and whatnot ughh
        if (left.Value.EndsWith("/"))
            return left.Value + right.Value;
        var leftSpan = left.Value.AsSpan();
        var last = leftSpan.LastIndexOf('/');
        Span<char> finalSpan = stackalloc char[last + right.Value.Length];
        leftSpan[..last].CopyTo(finalSpan);
        right.Value.AsSpan().CopyTo(finalSpan[last..]);
        return finalSpan.ToString();
    }

    public static implicit operator Path(string value) => new(value);

    public static Path operator /(Path left, Path right) => Combine(left, right);

}
