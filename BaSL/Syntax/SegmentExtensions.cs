using System;

namespace BaSL.Syntax;

internal static class SegmentExtensions
{

    extension(ReadOnlyMemory<Segment> segments)
    {

        public int FindIndex<T>(int start = 0) where T : Segment
        {
            for (var i = start; i < segments.Length; i++)
                if (segments.Span[i] is T)
                    return i;
            return -1;
        }

    }

}
