using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BaSL.Executables;

namespace BaSL.Syntax;

internal delegate bool TryParse(string variable, [NotNullWhen(true)] out string? result);

internal static class StatementParser
{

    private const string DefaultIfs = " \t\n";

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

    public static ShellStatement? CreateStatement(ReadOnlySpan<Segment> syntax) => syntax switch
    {
        [VariablesSegment {Variables: {Count: not 0} variables}] => new DeclareStatement(variables),
        [VariablesSegment {Variables: {Count: not 0} variables}, ArgsSegment {Args: var firstArgs}, .. var rest] => CreateStatement(rest, firstArgs, variables),
        [ArgsSegment {Args: var firstArgs}, .. var rest] => CreateStatement(rest, firstArgs),
        _ => null
    };

    private static ShellStatement? CreateStatement(ReadOnlySpan<Segment> syntax, Args firstArgs, Variables? variables = null)
    {
        if (StandaloneStatement.FromArgs(firstArgs) is not { } first)
            return null;
        if (variables is not null)
            first = first with {Variables = variables};
        return syntax switch // TODO: procedural?
        {
            [] => first,
            [StdinFileSegment, ArgsSegment {Args: [var source, ..]}] => first < source,
            [RedirectOverwriteSegment, ArgsSegment {Args: [var target, ..]}] => first > target,
            [RedirectAppendSegment, ArgsSegment {Args: [var target, ..]}] => first >> target,
            [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, RedirectOverwriteSegment, ArgsSegment {Args: [var target, ..]}] => first < source > target,
            [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, RedirectAppendSegment, ArgsSegment {Args: [var target, ..]}] => (first < source) >> target,
            [PipeSegment, ArgsSegment {Args: var targetArgs}] => first | targetArgs,
            [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, PipeSegment, ArgsSegment {Args: var targetArgs}] => first < source | targetArgs,
            [StdinFileSegment, ArgsSegment {Args: [var source, ..]}, PipeSegment, ..] => ExpandPipes(first < source, syntax, 4),
            [PipeSegment, ..] => ExpandPipes((first), syntax),
            _ => null
        };
    }

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

    private static int ParseStatements(ReadOnlySpan<char> s, int start, List<Segment> statements, TryParse local, string home)
    {
        var argBuzilder = new StringBuilder();
        var variableBuilder = new StringBuilder();
        var args = new List<string>();
        var syntax = SyntaxType.Text;
        var outerSyntax = SyntaxType.Text;
        var condition = false;
        Variables? vars = null;
        var potentialVar = true;
        var raw = true;
        string? varName = null;
        for (var i = start; i < s.Length; i++)
        {
            var c = s[i];
            char? next = i < s.Length - 1 ? s[i + 1] : null;
            switch (syntax, c, next)
            {
                case (not SyntaxType.VerbatimString, '\\', _):
                    i++;
                    argBuzilder.Append(next);
                    raw = potentialVar = false;
                    break;
                case (SyntaxType.VerbatimString, '\'', _):
                case (SyntaxType.QuotedString, '"', _):
                    syntax = outerSyntax = SyntaxType.Text;
                    raw = potentialVar = false;
                    break;
                case (SyntaxType.Text, '\'', _):
                    syntax = SyntaxType.VerbatimString;
                    raw = potentialVar = false;
                    break;
                case (SyntaxType.Text, '"', _):
                    syntax = SyntaxType.QuotedString;
                    raw = potentialVar = false;
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
                    raw = false;
                    break;
                case (SyntaxType.Variable, ';', _) when outerSyntax == SyntaxType.Text:
                    AppendVariable();
                    Complete();
                    return i;
                case (SyntaxType.Variable, ' ', _) when outerSyntax == SyntaxType.QuotedString:
                case (SyntaxType.Variable, '.', _):
                    AddArg(space: c == ' ');
                    argBuzilder.Append(c);
                    break;
                case (SyntaxType.Variable, '"', _) when outerSyntax == SyntaxType.QuotedString:
                case (SyntaxType.Variable, ' ', _):
                    AppendVariable();
                    AddArg(space: c == ' ');
                    break;
                case (SyntaxType.Variable, _, _):
                    variableBuilder.Append(c);
                    break;
                case (SyntaxType.Text, '=', _) when raw && potentialVar && outerSyntax == SyntaxType.Text:
                    varName = argBuzilder.ToString();
                    argBuzilder.Clear();
                    break;
                case (SyntaxType.Text, '~', _) when !string.IsNullOrEmpty(home) && argBuzilder.Length == 0:
                    argBuzilder.Append(home);
                    raw = false;
                    break;
                case (SyntaxType.Text, _, _) when char.IsWhiteSpace(c):
                    AddArg();
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
            if (varName != null)
            {
                vars ??= [];
                vars[varName] = argBuzilder.ToString();
                raw = true;
            }
            else if (argBuzilder.Length != 0)
            {
                var arg = argBuzilder.ToString();
                if (space && args.Count == 0 && KeywordSegment.Get(arg) is { } keyword)
                    statements.Add(keyword);
                else
                    args.Add(arg);
            }

            argBuzilder.Clear();
            syntax = next;
            varName = null;
        }

        void AddStatement(Segment? segment = null)
        {
            AddArg();
            raw = potentialVar = false;
            if (vars is {Count: not 0})
            {
                statements.Add(new VariablesSegment(vars));
                vars = null;
            }

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
            else if (local(variableBuilder.ToString(), out var result))
            {
                if (outerSyntax != SyntaxType.Text)
                    argBuzilder.Append(result);
                else
                {
                    var separator = local("IFS", out var ifs) ? ifs : DefaultIfs;
                    foreach (var memory in result.AsMemory().Split(separator.AsMemory()))
                        args.Add(memory.Span.ToString());
                }
            }

            variableBuilder.Clear();
            syntax = outerSyntax;
        }

        void Complete(Continue @continue = Continue.Always)
        {
            if (argBuzilder.Length != 0)
                AddArg();
            if (vars is {Count: not 0})
                statements.Add(new VariablesSegment(vars));
            if (args.Count != 0)
                AddStatement();
            statements.Add(new ContinueSegment(@continue));
        }
    }

}
