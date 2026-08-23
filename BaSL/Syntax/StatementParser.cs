using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BaSL.Syntax;

internal delegate bool TryParse(string variable, [NotNullWhen(true)] out string? result);

internal static class StatementParser
{

    private static readonly char[] DefaultIfs = [' ', '\t', '\n'];

    [ThreadStatic]
    private static List<Segment>? _segments;

    public static ReadOnlyMemory<Segment> Parse(string line, TryParse variables, string home)
    {
        _segments ??= [];
        var index = -1;
        do
        {
            index = ParseStatements(line, index + 1, _segments, variables, home);
        }
        while (index != -1);

        if (_segments is [.., ContinueSegment {On: Continue.Always}])
            _segments.RemoveAt(_segments.Count - 1);
        var array = _segments.ToArray();
        _segments.Clear();
        return array;
    }

    public static ShellStatement? CreateStatement(ReadOnlySpan<Segment> syntax)
        => syntax is not [ArgsSegment {Args: var firstArgs}, ..]
            ? null
            : syntax[1..] switch // TODO: procedural?
            {
                [] => StandaloneStatement.FromArgs(firstArgs),
                [StdinFileSegment, ArgsSegment {Args: [var source, ..]}] => firstArgs < source,
                [RedirectOverwriteSegment, ArgsSegment {Args: [var target, ..]}] => firstArgs > target,
                [RedirectAppendSegment, ArgsSegment {Args: [var target, ..]}] => firstArgs >> target,
                [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, RedirectOverwriteSegment, ArgsSegment {Args: [var target, ..]}] => firstArgs < source > target,
                [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, RedirectAppendSegment, ArgsSegment {Args: [var target, ..]}] => (firstArgs < source) >> target,
                [PipeSegment, ArgsSegment {Args: var targetArgs}] => firstArgs | targetArgs,
                [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, PipeSegment, ArgsSegment {Args: var targetArgs}] => firstArgs < source | targetArgs,
                [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, PipeSegment, ..] => ExpandPipes(StandaloneStatement.FromArgs(firstArgs) < source, syntax, 4),
                [PipeSegment, ..] => ExpandPipes(StandaloneStatement.FromArgs(firstArgs), syntax),
                _ => null
            };

    private static ShellStatement? ExpandPipes(ShellStatement? statement, ReadOnlySpan<Segment> syntax, int stardIndex = 2)
    {
        for (var i = stardIndex; i < syntax.Length; i += 2)
        {
            if (statement is not ExtendableStatement || syntax[i] is not ArgsSegment {Args: var args})
                return null;
            switch (syntax[i - 1], args.IsEmpty)
            {
                case (PipeSegment, _):
                    statement |= StandaloneStatement.FromArgs(args);
                    break;
                case (RedirectOverwriteSegment, false):
                    return statement > args[0];
                case (RedirectAppendSegment, false):
                    return statement >> args[0];
                default:
                    return null;
            }
        }

        return statement;
    }

    private static int ParseStatements(ReadOnlySpan<char> s, int start, List<Segment> statements, TryParse variables, string home)
    {
        var argBuzilder = new StringBuilder();
        var variableBuilder = new StringBuilder();
        var args = new List<string>();
        var syntax = SyntaxType.Text;
        var outerSyntax = SyntaxType.Text;
        var condition = false;
        for (var i = start; i < s.Length; i++)
        {
            var c = s[i];
            char? next = i < s.Length - 1 ? s[i + 1] : null;
            switch (syntax, c, next)
            {
                case (not SyntaxType.VerbatimString, '\\', _):
                    i++;
                    argBuzilder.Append(next);
                    break;
                case (SyntaxType.VerbatimString, '\'', _):
                case (SyntaxType.QuotedString, '"', _):
                    syntax = outerSyntax = SyntaxType.Text;
                    break;
                case (SyntaxType.Text, '\'', _):
                    syntax = SyntaxType.VerbatimString;
                    break;
                case (SyntaxType.Text, '"', _):
                    syntax = SyntaxType.QuotedString;
                    break;
                case (SyntaxType.Text, '|', '|') when !condition:
                    Complete(Continue.OnFailure);
                    return i + 1;
                case (SyntaxType.Text, '&', '&') when !condition:
                    Complete(Continue.OnSuccess);
                    return i + 1;
                case (SyntaxType.Text, ';', _) when !condition:
                    Complete();
                    return i;
                case (SyntaxType.Text, '|', _) when !condition:
                    AddStatement(Segment.Pipe);
                    break;
                case (SyntaxType.Text, '>', '>') when !condition:
                    AddStatement(Segment.RedirectAppend);
                    i++;
                    break;
                case (SyntaxType.Text, '>', _) when !condition:
                    AddStatement(Segment.RedirectOverwrite);
                    break;
                case (SyntaxType.Text, '<', _) when !condition:
                    AddStatement(Segment.StdinFile);
                    break;
                case (SyntaxType.Text, '[', '[') when argBuzilder.Length == 0:
                    i++;
                    AddStatement(KeywordSegment.BeginCondition);
                    condition = true;
                    break;
                case (SyntaxType.Text, ']', ']') when argBuzilder.Length == 0:
                    i++;
                    AddStatement(KeywordSegment.EndCondition);
                    condition = false;
                    break;
                case (SyntaxType.Text or SyntaxType.QuotedString, '$', _):
                    syntax = SyntaxType.Variable;
                    break;
                case (SyntaxType.Variable, ';', _) when outerSyntax == SyntaxType.Text:
                    AppendVariable();
                    Complete();
                    return i;
                case (SyntaxType.Variable, ' ', _) when outerSyntax == SyntaxType.QuotedString:
                case (SyntaxType.Variable, '.', _):
                    AddArg();
                    argBuzilder.Append(c);
                    break;
                case (SyntaxType.Variable, '"', _) when outerSyntax == SyntaxType.QuotedString:
                case (SyntaxType.Variable, ' ', _):
                    AppendVariable();
                    AddArg();
                    break;
                case (SyntaxType.Variable, _, _):
                    variableBuilder.Append(c);
                    break;
                case (SyntaxType.Text, '~', _) when !string.IsNullOrEmpty(home) && argBuzilder.Length == 0:
                    argBuzilder.Append(home);
                    break;
                case (SyntaxType.Text, _, _) when char.IsWhiteSpace(c):
                    AddArg(space: true);
                    break;
                case (SyntaxType.Text or SyntaxType.QuotedString or SyntaxType.VerbatimString, _, _):
                    argBuzilder.Append(c);
                    break;
            }
        }

        Complete();
        return -1;

        void AddArg(SyntaxType next = SyntaxType.Text, bool space = true)
        {
            if (syntax == SyntaxType.Variable)
                AppendVariable();
            if (argBuzilder.Length != 0)
            {
                var arg = argBuzilder.ToString();
                if (space && args.Count == 0 && KeywordSegment.Get(arg) is { } keyword)
                    statements.Add(keyword);
                else
                    args.Add(arg);
            }

            argBuzilder.Clear();
            syntax = next;
        }

        void AddStatement(Segment? segment = null)
        {
            AddArg();
            if (args.Count != 0)
                statements.Add(new ArgsSegment(args.ToArray()));
            if (segment is not null)
                statements.Add(segment);
            args.Clear();
            outerSyntax = syntax = SyntaxType.Text;
        }

        void AppendVariable()
        {
            if (variableBuilder.Length == 0)
                argBuzilder.Append('$');
            else if (variables(variableBuilder.ToString(), out var result))
            {
                if (outerSyntax == SyntaxType.Text)
                    argBuzilder.Append(result);
                else
                    args.AddRange(result.Split(DefaultIfs));
            }

            variableBuilder.Clear();
            syntax = outerSyntax;
        }

        void Complete(Continue @continue = Continue.Always)
        {
            if (argBuzilder.Length != 0)
                AddArg();
            if (args.Count != 0)
                AddStatement();
            statements.Add(new ContinueSegment(@continue));
        }
    }

}
