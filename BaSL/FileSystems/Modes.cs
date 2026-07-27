using System;

namespace BaSL.FileSystems;

public readonly record struct Modes(Mode Owner, Mode Group, Mode Others);

public static class ModesExtensions
{

    extension(Modes)
    {

        public static bool TryParseOctal(ReadOnlySpan<char> span, out Modes modes)
        {
            span = span.Trim();
            if (span.Length != 3
                || !Mode.TryParseOctal(span[0], out var owner)
                || !Mode.TryParseOctal(span[1], out var group)
                || !Mode.TryParseOctal(span[2], out var others)
               )
            {
                modes = default;
                return false;
            }

            modes = new Modes(owner, group, others);
            return true;
        }

        public static Modes ParseOctal(ReadOnlySpan<char> span)
            // ReSharper disable once InvokeAsExtensionMemberFromSameClass
            => TryParseOctal(span, out var modes)
                ? modes
                : throw new ArgumentException("Must provide exactly 3 digits");

    }

    extension(Modes modes)
    {

        public string ToOctal() => $"{modes.Owner.ToOctal()}{modes.Group.ToOctal()}{modes.Others.ToOctal()}";

    }

}
