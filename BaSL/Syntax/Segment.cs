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
