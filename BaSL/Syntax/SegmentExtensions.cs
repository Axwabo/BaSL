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

        public int FindIndex<T1, T2>(int start = 0) where T1 : Segment where T2 : Segment
        {
            for (var i = start; i < segments.Length; i++)
                if (segments.Span[i] is T1 or T2)
                    return i;
            return -1;
        }

        public int FindIndex(Segment segment, int start = 0)
        {
            var index = segments.Span[start..].IndexOf(segment);
            return index == -1 ? -1 : index + start;
        }

    }

    extension(ContinueSegment segment)
    {

        public bool Exit(int code) => segment.On switch
        {
            Continue.OnFailure => code == 0,
            Continue.OnSuccess => code != 0,
            _ => false
        };

    }

}
