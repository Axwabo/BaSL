using System;
using System.Collections;
using System.Collections.Generic;

namespace BaSL.Executables;

public readonly record struct SplitReadOnlyMemory<T>(ReadOnlyMemory<T> Memory, ReadOnlyMemory<T> Split) : IEnumerable<ReadOnlyMemory<T>> where T : IEquatable<T>
{

    public IEnumerator<ReadOnlyMemory<T>> GetEnumerator() => new SplitReadOnlyMemoryEnumerator<T>(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}

public struct SplitReadOnlyMemoryEnumerator<T> : IEnumerator<ReadOnlyMemory<T>> where T : IEquatable<T>
{

    private readonly SplitReadOnlyMemory<T> _memory;

    private int _index = -1;

    public SplitReadOnlyMemoryEnumerator(SplitReadOnlyMemory<T> memory) => _memory = memory;

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        var startIndex = _index + 1;
        if (startIndex >= _memory.Memory.Length)
            return false;
        var next = _memory.Memory.FindIndexAny(_memory.Split.Span, startIndex);
        Current = next == -1
            ? _memory.Memory[(next + 1)..]
            : _memory.Memory[Math.Max(0, _index)..(next + 1)];
        _index = next == -1 ? _memory.Memory.Length : next;
        return true;
    }

    public void Reset() => _index = -1;

    public ReadOnlyMemory<T> Current { get; private set; }

    object IEnumerator.Current => Current;

}
