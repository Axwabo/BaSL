using System;

namespace BaSL.Executables;

public static class MemoryExtensions
{

    extension<T>(ReadOnlyMemory<T> memory) where T : IEquatable<T>
    {

        public ReadOnlyMemoryEnumerator<T> GetEnumerator() => new(memory);

        public int FindIndex(T item, int start = 0)
        {
            var index = memory.Span[start..].IndexOf(item);
            return index == -1 ? -1 : index + start;
        }

        public int FindIndexAny(ReadOnlySpan<T> items, int start = 0)
        {
            var index = memory.Span[start..].IndexOfAny(items);
            return index == -1 ? -1 : index + start;
        }

        public SplitReadOnlyMemory<T> Split(params T[] split) => new(memory, split);

    }

    extension<T>(ReadOnlyMemory<T> memory) where T : notnull
    {

        public T? FirstOrDefault() => memory.Length == 0 ? default : memory.Span[0];

        public T FirstOrDefault(T @default) => memory.Length == 0 ? @default : memory.Span[0];

    }

}
