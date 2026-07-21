using System;
using System.Collections;
using System.Collections.Generic;

namespace BaSL.Executables;

public static class MemoryExtensions
{

    extension<T>(ReadOnlyMemory<T> memory)
    {

        public ReadOnlyMemoryEnumerator<T> GetEnumerator() => new(memory);

    }

}

public struct ReadOnlyMemoryEnumerator<T> : IEnumerator<T>
{

    private readonly ReadOnlyMemory<T> _memory;

    private int _index = -1;

    public ReadOnlyMemoryEnumerator(ReadOnlyMemory<T> memory) => _memory = memory;

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        var newIndex = _index + 1;
        if (newIndex >= _memory.Length)
            return false;
        _index = newIndex;
        return true;
    }

    public void Reset() => _index = -1;

    public T Current => _memory.Span[_index];

    object? IEnumerator.Current => Current;

}
