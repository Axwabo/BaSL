using System;

namespace BaSL.FileSystems;

[Flags]
public enum Mode
{

    None = 0,
    Execute = 1 << 0,
    Write = 1 << 1,
    Read = 1 << 2,
    Rw = Read | Write,
    Rx = Read | Execute,
    Wx = Write | Execute,
    Rwx = Read | Write | Execute

}

public static class ModeExtensions
{

    extension(Mode)
    {

        public static bool TryParseOctal(char c, out Mode mode)
        {
            Mode? nullableMode = c switch
            {
                '0' => Mode.None,
                '1' => Mode.Execute,
                '2' => Mode.Write,
                '3' => Mode.Wx,
                '4' => Mode.Read,
                '5' => Mode.Rx,
                '6' => Mode.Rw,
                '7' => Mode.Rwx,
                _ => null
            };
            mode = nullableMode ?? Mode.None;
            return nullableMode.HasValue;
        }

        public static Mode ParseOctal(char c)
            // ReSharper disable once InvokeAsExtensionMemberFromSameClass
            => TryParseOctal(c, out var mode)
                ? mode
                : throw new ArgumentOutOfRangeException(nameof(c), c, "Unknown mode");

    }

    extension(Mode mode)
    {

        public bool CanRead => mode.Has(Mode.Read);

        public bool CanWrite => mode.Has(Mode.Write);

        public bool CanExecute => mode.Has(Mode.Execute);

        public char ToOctal() => mode switch
        {
            Mode.None => '0',
            Mode.Execute => '1',
            Mode.Write => '2',
            Mode.Wx => '3',
            Mode.Read => '4',
            Mode.Rx => '5',
            Mode.Rw => '6',
            Mode.Rwx => '7',
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown mode")
        };

        public bool Has(Mode other) => (mode & other) == other;

    }

}
