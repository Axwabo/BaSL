using System;
using System.Collections;
using System.Collections.Generic;

namespace BaSL.SourceGenerators;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
{

    public static readonly EquatableArray<T> Empty = new([]);

    private readonly T[]? _array;

    public EquatableArray(T[] array) => _array = array;

    public bool Equals(EquatableArray<T> other)
    {
        var thisSpan = AsSpan();
        var otherSpan = AsSpan();
        if (thisSpan.Length != otherSpan.Length)
            return false;
        for (var i = 0; i < thisSpan.Length; i++)
            if (!EqualityComparer<T>.Default.Equals(thisSpan[i], otherSpan[i]))
                return false;
        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        HashCode
    }

    public ReadOnlySpan<T> AsSpan() => _array;

    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
