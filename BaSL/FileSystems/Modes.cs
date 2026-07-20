using System;

namespace BaSL.FileSystems;

public readonly record struct Modes(Mode Owner, Mode Group, Mode Others);

public static class ModesExtensions
{

    extension(Modes modes)
    {

        public static Modes ParseOctal(ReadOnlySpan<char> span)
        {
            span = span.Trim();
            return span.Length == 3
                ? new Modes(Mode.ParseOctal(span[0]), Mode.ParseOctal(span[1]), Mode.ParseOctal(span[2]))
                : throw new ArgumentException("Must provide exactly 3 digits");
        }

        public string ToOctal() => $"{modes.Owner.ToOctal()}{modes.Group.ToOctal()}{modes.Others.ToOctal()}";

    }

}
