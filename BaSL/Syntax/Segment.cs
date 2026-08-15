using System;

namespace BaSL.Syntax;

internal abstract record Segment
{

    public static Segment Pipe { get; } = new PipeSegment();

    public static Segment StdinFile { get; } = new StdinFileSegment();

    public static Segment RedirectOverwrite { get; } = new RedirectOverwriteSegment();

    public static Segment RedirectAppend { get; } = new RedirectAppendSegment();

}

internal sealed record ArgsSegment(Args Args) : Segment;

internal sealed record PipeSegment : Segment;

internal sealed record StdinFileSegment : Segment;

internal sealed record RedirectOverwriteSegment : Segment;

internal sealed record RedirectAppendSegment : Segment;

internal sealed record ContinueSegment(Continue On) : Segment
{

    public static ContinueSegment Always { get; } = new(Continue.Always);

}

internal sealed record KeywordSegment(Keyword Keyword) : Segment
{

    public static KeywordSegment? Get(ReadOnlySpan<char> span) => span switch
    {
        "if" => If,
        "then" => Then,
        "else" => Else,
        "fi" => EndIf,
        "[[" => BeginCondition,
        "]]" => EndCondition,
        _ => null
    };

    public static KeywordSegment If { get; } = new(Keyword.If);
    public static KeywordSegment Then { get; } = new(Keyword.Then);
    public static KeywordSegment Else { get; } = new(Keyword.Else);
    public static KeywordSegment EndIf { get; } = new(Keyword.EndIf);
    public static KeywordSegment BeginCondition { get; } = new(Keyword.BeginCondition);
    public static KeywordSegment EndCondition { get; } = new(Keyword.EndCondition);

}

internal sealed record OperatorSegment(Operator Operator) : Segment
{

    public static OperatorSegment Eq { get; } = new(Operator.Equals);
    public static OperatorSegment NotEq { get; } = new(Operator.NotEquals);
    public static OperatorSegment LeftGreater { get; } = new(Operator.LeftGreaterThanRight);
    public static OperatorSegment LeftLess { get; } = new(Operator.LeftLessThanRight);

}
