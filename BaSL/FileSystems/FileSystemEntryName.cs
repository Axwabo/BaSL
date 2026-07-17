using System;

namespace BaSL.FileSystems;

public readonly record struct FileSystemEntryName
{

    public static bool IsValid(ReadOnlySpan<char> name) => name.IndexOf('/') == -1;

    public static void ThrowIfInvalid(ReadOnlySpan<char> name)
    {
        if (!IsValid(name))
            throw new ArgumentException("Forward slash not allowed");
    }

    public string Value { get; }

    public FileSystemEntryName(string value)
    {
        ThrowIfInvalid(value);
        Value = value;
    }

    public FileSystemEntryName(ReadOnlySpan<char> value)
    {
        ThrowIfInvalid(value);
        Value = value.ToString();
    }

    public static implicit operator FileSystemEntryName(string value) => new(value);

}
