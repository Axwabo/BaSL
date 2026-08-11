using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BaSL.Syntax;

internal delegate bool TryParse(string variable, [NotNullWhen(true)] out string? result);

internal static class StatementParser
{

    public static List<Segment> Parse(string line, TryParse variables, string home)
    {
        var syntax = new List<Segment>();
        var index = -1;
        do
        {
            index = ParseStatements(line.AsSpan(index + 1), syntax, variables, home);
        }
        while (index != -1);

        return syntax;
    }

    private static ShellStatement? CreateStatement(List<Segment> syntax, Args firstArgs) => syntax switch
    {
        [_] => StandaloneStatement.FromArgs(firstArgs),
        [_, StdinFileSegment, ArgsSegment {Args: [var source, ..]}] => firstArgs < source,
        [_, RedirectOverwriteSegment, ArgsSegment {Args: [var target, ..]}] => firstArgs > target,
        [_, RedirectAppendSegment, ArgsSegment {Args: [var target, ..]}] => firstArgs >> target,
        [_, StdinFileSegment, ArgsSegment {Args: [var source, ..]}, RedirectOverwriteSegment, ArgsSegment {Args: [var target, ..]}] => firstArgs < source > target,
        [_, StdinFileSegment, ArgsSegment {Args: [var source, ..]}, RedirectAppendSegment, ArgsSegment {Args: [var target, ..]}] => (firstArgs < source) >> target,
        [_, PipeSegment, ArgsSegment {Args: var targetArgs}] => firstArgs | targetArgs,
        [_, StdinFileSegment, ArgsSegment {Args: [var source, ..]}, PipeSegment, ArgsSegment {Args: var targetArgs}] => firstArgs < source | targetArgs,
        [_, StdinFileSegment, ArgsSegment {Args: [var source, ..]}, PipeSegment, ..] => ExpandPipes(StandaloneStatement.FromArgs(firstArgs) < source, syntax, 4),
        [_, PipeSegment, ..] => ExpandPipes(StandaloneStatement.FromArgs(firstArgs), syntax),
        _ => null
    };

    private static ShellStatement? ExpandPipes(ShellStatement? statement, List<Segment> syntax, int stardIndex = 2)
    {
        for (var i = stardIndex; i < syntax.Count; i += 2)
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

    private static int ParseStatements(ReadOnlySpan<char> s, List<Segment> statements, TryParse variables, string home)
    {
        var argBuzilder = new StringBuilder();
        var variableBuilder = new StringBuilder();
        var args = new List<string>();
        var syntax = SyntaxType.Text;
        var outerSyntax = SyntaxType.Text;
        var escaped = false;
        var raw = true;
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (escaped)
            {
                argBuzilder.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            char? next = i < s.Length - 1 ? s[i + 1] : null;
            switch (syntax, c)
            {
                case (SyntaxType.VerbatimString, '\''):
                case (SyntaxType.QuotedString, '"'):
                    syntax = outerSyntax = SyntaxType.Text;
                    raw = false;
                    break;
                case (SyntaxType.Text, '\''):
                    syntax = SyntaxType.VerbatimString;
                    break;
                case (SyntaxType.Text, '"'):
                    syntax = SyntaxType.QuotedString;
                    break;
                case (SyntaxType.Text, '|') when next == '|':
                    Complete(Continue.OnFailure);
                    return i + 1;
                case (SyntaxType.Text, '&') when next == '&':
                    Complete(Continue.OnSuccess);
                    return i + 1;
                case (SyntaxType.Text, ';'):
                    Complete();
                    return i;
                case (SyntaxType.Text, '|'):
                    AddStatement(Segment.Pipe);
                    break;
                case (SyntaxType.Text, '>') when next == '>':
                    AddStatement(Segment.RedirectAppend);
                    i++;
                    break;
                case (SyntaxType.Text, '>'):
                    AddStatement(Segment.RedirectOverwrite);
                    break;
                case (SyntaxType.Text, '<'):
                    AddStatement(Segment.StdinFile);
                    break;
                case (SyntaxType.Text or SyntaxType.QuotedString, '$'):
                    raw = false;
                    outerSyntax = syntax;
                    syntax = SyntaxType.Variable;
                    break;
                case (SyntaxType.Variable, ';') when outerSyntax == SyntaxType.Text:
                    AppendVariable();
                    Complete();
                    return i;
                case (SyntaxType.Variable, '"') when outerSyntax == SyntaxType.QuotedString:
                    AddArg();
                    break;
                case (SyntaxType.Variable, ' ' or '.'):
                    AppendVariable();
                    argBuzilder.Append(c);
                    break;
                case (SyntaxType.Variable, _):
                    variableBuilder.Append(c);
                    break;
                case (SyntaxType.Text, '~') when !string.IsNullOrEmpty(home) && argBuzilder.Length == 0:
                    raw = false;
                    argBuzilder.Append(home);
                    break;
                case (SyntaxType.Text, _) when char.IsWhiteSpace(c):
                    AddArg();
                    break;
                case (SyntaxType.Text or SyntaxType.QuotedString or SyntaxType.VerbatimString, _):
                    argBuzilder.Append(c);
                    break;
            }
        }

        Complete();
        return -1;

        void AddArg(SyntaxType next = SyntaxType.Text)
        {
            if (syntax == SyntaxType.Variable)
                AppendVariable();
            if (argBuzilder.Length != 0)
                args.Add(argBuzilder.ToString());
            argBuzilder.Clear();
            outerSyntax = syntax;
            syntax = next;
        }

        void AddStatement(Segment? segment = null)
        {
            // TODO: maybe this doesn't belong here
            if (raw && args.Count == 1 && KeywordSegment.Get(args[0]) is { } keyword)
                statements.Add(keyword);
            else
                statements.Add(new ArgsSegment(args.ToArray()));
            raw = true;
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
                argBuzilder.Append(result);
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
