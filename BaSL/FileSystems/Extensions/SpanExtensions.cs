using System;

namespace BaSL.FileSystems.Extensions;

public static class SpanExtensions
{

    extension(ReadOnlySpan<char> span)
    {

        public int IndexOf(char item, int start)
        {
            if (start <= 0)
                return span.IndexOf(item);
            var index = span[start..].IndexOf(item);
            return index == -1 ? -1 : index + start;
        }

    }

}
