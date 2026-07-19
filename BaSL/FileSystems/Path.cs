using System;

namespace BaSL.FileSystems;

public readonly record struct Path(string Value)
{

    public static Path Root { get; } = "/";

    public static Path Combine(Path left, Path right)
    {
        var leftSpan = left.Value.AsSpan();
        var rightSpan = right.Value.AsSpan();
        // TODO: validation and whatnot ughh
        if (leftSpan.EndsWith("/") || rightSpan.StartsWith("/"))
            return left.Value + right.Value;
        var length = leftSpan.Length;
        Span<char> span = stackalloc char[length + rightSpan.Length + 1];
        span[length] = '/';
        leftSpan.CopyTo(span);
        rightSpan.CopyTo(span[(length + 1)..]);
        return span.ToString();
    }

    public Path ToAbsolutePath(Path basePath)
    {
        var baseSpan = basePath.Value.AsSpan();
        return baseSpan.IsEmpty || Value.AsSpan().StartsWith("/")
            ? this
            : Combine(basePath, this);
    }

    public static implicit operator Path(string value) => new(value);

    public static implicit operator Path(FileSystemEntryName name) => new(name.Value);

    public static Path operator /(Path left, Path right) => Combine(left, right);

}
