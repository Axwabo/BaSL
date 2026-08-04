using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BaSL.Syntax;

internal delegate bool TryParse(string variable, [NotNullWhen(true)] out string? result);

internal static class StatementParser
{

    // TODO: escaping & shit
    public static List<ShellStatement?> Parse(string line, TryParse variables)
    {
        var results = new List<ShellStatement?>();
        var syntax = new List<Segment>();
        ParseStatements(line, syntax, variables);
        results.Add(syntax switch
        {
            [ArgsSegment {Args: var simpleArgs}] => StandaloneStatement.FromArgs(simpleArgs),
            [ArgsSegment {Args: var sourceArgs}, RedirectOverwriteSegment, ArgsSegment {Args: [var target]}] => sourceArgs > target,
            [ArgsSegment {Args: var sourceArgs}, RedirectAppendSegment, ArgsSegment {Args: [var target]}] => sourceArgs >> target,
            [ArgsSegment {Args: var sourceArgs}, PipeSegment, ArgsSegment {Args: var targetArgs}] => sourceArgs | targetArgs,
            [ArgsSegment {Args: var firstArgs}, PipeSegment, ..] => ExpandPipes(firstArgs, syntax),
            _ => null
        });

        return results;
    }

    private static ShellStatement? ExpandPipes(Args firstArgs, List<Segment> syntax)
    {
        ShellStatement? statement = StandaloneStatement.FromArgs(firstArgs);
        for (var i = 2; i < syntax.Count; i += 2)
        {
            if (statement is not ExtendableStatement || syntax[i] is not ArgsSegment {Args: var args})
                return null;
            switch (syntax[i - 1])
            {
                case PipeSegment:
                    statement |= StandaloneStatement.FromArgs(args);
                    break;
                case RedirectOverwriteSegment:
                    return args.IsEmpty ? null : statement > args[0];
                case RedirectAppendSegment:
                    return args.IsEmpty ? null : statement >> args[0];
                default:
                    return null;
            }
        }

        return statement;
    }

    private static void ParseStatements(string s, List<Segment> statements, TryParse variables)
    {
        var argBuzilder = new StringBuilder();
        var variableBuilder = new StringBuilder();
        var args = new List<string>();
        var syntax = SyntaxType.Text;
        var outerSyntax = SyntaxType.Text;
        var escaped = false;
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
                    AddArg();
                    break;
                case (SyntaxType.Text, '\''):
                    AddArg(SyntaxType.VerbatimString);
                    break;
                case (SyntaxType.Text, '"'):
                    AddArg(SyntaxType.QuotedString);
                    break;
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
                case (SyntaxType.Text or SyntaxType.QuotedString, '$'):
                    outerSyntax = syntax;
                    syntax = SyntaxType.Variable;
                    break;
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
                case (SyntaxType.Text, _) when char.IsWhiteSpace(c):
                    AddArg();
                    break;
                case (SyntaxType.Text or SyntaxType.QuotedString or SyntaxType.VerbatimString, _):
                    argBuzilder.Append(c);
                    break;
            }
        }

        if (argBuzilder.Length != 0)
            AddArg();
        if (args.Count != 0)
            AddStatement();

        return;

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
                argBuzilder.Append(result);
            variableBuilder.Clear();
            syntax = outerSyntax;
        }
    }

}
