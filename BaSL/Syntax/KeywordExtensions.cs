using System;

namespace BaSL.Syntax;

internal static class KeywordExtensions
{

    public const string If = "if";
    public const string Then = "then";
    public const string Else = "else";
    public const string ElseIf = "elif";
    public const string EndIf = "fi";
    public const string BeginCondition = "[[";
    public const string EndCondition = "]]";

    extension(Keyword keyword)
    {

        public string Token => keyword switch
        {
            Keyword.If => If,
            Keyword.Then => Then,
            Keyword.Else => Else,
            Keyword.ElseIf => ElseIf,
            Keyword.EndIf => EndIf,
            Keyword.BeginCondition => BeginCondition,
            Keyword.EndCondition => EndCondition,
            _ => throw new ArgumentOutOfRangeException(nameof(keyword), keyword, "Unknown keyword")
        };

    }

    extension(KeywordSegment)
    {

        public static KeywordSegment? Get(ReadOnlySpan<char> span) => span switch
        {
            If => KeywordSegment.If,
            Then => KeywordSegment.Then,
            Else => KeywordSegment.Else,
            ElseIf => KeywordSegment.ElseIf,
            EndIf => KeywordSegment.EndIf,
            BeginCondition => KeywordSegment.BeginCondition,
            EndCondition => KeywordSegment.EndCondition,
            _ => null
        };

    }

}
