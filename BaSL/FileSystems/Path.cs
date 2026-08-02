using System;
using BaSL.FileSystems.Extensions;

namespace BaSL.FileSystems;

public readonly record struct Path(string Value)
{

    public static Path Combine(Path left, Path right)
    {
        var leftSpan = left.Value.AsSpan();
        var rightSpan = right.Value.AsSpan();
        // TODO: validation and whatnot ughh
        return leftSpan.EndsWith("/") || rightSpan.StartsWith("/")
            ? left.Value + right.Value
            : Combine(leftSpan, rightSpan);
    }

    private static Path Combine(ReadOnlySpan<char> leftSpan, ReadOnlySpan<char> rightSpan)
    {
        var length = leftSpan.Length;
        Span<char> span = stackalloc char[length + rightSpan.Length + 1];
        span[length] = '/';
        leftSpan.CopyTo(span);
        rightSpan.CopyTo(span[(length + 1)..]);
        return span.ToString();
    }

    public static implicit operator Path(string value) => new(value);

    public static implicit operator Path(FileSystemEntryName name) => new(name.Value);

    public static implicit operator ReadOnlyMemory<char>(Path path) => path.Value.AsMemory();

    public static Path operator /(Path left, Path right) => Combine(left, right);

    public static Path Root { get; } = "/";

    public static Path Binaries { get; } = "/usr/bin";

    /// <summary>
    /// Creates an absolute path that is usable in Resolve methods.
    /// </summary>
    /// <param name="basePath">The path to make this path relative to.</param>
    /// <returns>The current path if it's absolute or if the <paramref name="basePath"/> is empty, otherwise, a combined path with <paramref name="basePath"/> as the left operand.</returns>
    /// <remarks>Self and parent (<c>.</c> and <c>..</c>) markers are retained.</remarks>
    public Path ToPartialAbsolute(Path basePath)
    {
        if (basePath.IsEmpty || this.IsAbsolute)
            return this;
        // TODO: don't duplicate common part
        return Combine(basePath, this);
    }

    public Path ToAbsolute(Path basePath)
    {
        if (basePath.IsEmpty || this.IsAbsolute)
            return this;
        var common = Path.GetCommonAncestor(Value, basePath.Value);
    }

}
