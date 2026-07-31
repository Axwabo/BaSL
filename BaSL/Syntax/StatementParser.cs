using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static BaSL.Syntax.StatementType;

namespace BaSL.Syntax;

internal delegate bool TryParse(string variable, [NotNullWhen(true)] out string? result);

internal static class StatementParser
{

    // TODO: escaping & shit
    public static List<ShellStatement?> Parse(string line, TryParse variables)
    {
        var results = new List<ShellStatement?>();
        var syntax = new List<Statement>();
        foreach (var s in line.Split(';'))
        {
            syntax.Clear();
            ParseStatements(s, syntax, variables);
            results.Add(syntax switch
            {
                [{Type: Simple, Args: var simpleArgs}] => StandaloneStatement.FromArgs(simpleArgs),
                [{Type: RedirectStandardOutputOverwrite, Args: var sourceArgs}, {Type: Simple, Args: [var target]}] => StandaloneStatement.FromArgs(sourceArgs) > target,
                [{Type: RedirectStandardOutputAppend, Args: var sourceArgs}, {Type: Simple, Args: [var target]}] => StandaloneStatement.FromArgs(sourceArgs) >> target,
                [{Type: Pipe, Args: var sourceArgs}, {Type: Simple, Args: var targetArgs}] => StandaloneStatement.FromArgs(sourceArgs) | StandaloneStatement.FromArgs(targetArgs),
                _ => null
            });
        }

        return results;
    }

    private static void ParseStatements(string s, List<Statement> statements, TryParse variables)
    {
        var argBuzilder = new StringBuilder();
        var variableBuilder = new StringBuilder();
        var args = new List<string>();
        var syntax = SyntaxType.Text;
        var outerSyntax = SyntaxType.Text;
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
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
                    AddStatement(Pipe);
                    break;
                case (SyntaxType.Text, '>') when next == '>':
                    AddStatement(RedirectStandardOutputAppend);
                    i++;
                    break;
                case (SyntaxType.Text, '>'):
                    AddStatement(RedirectStandardOutputOverwrite);
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
            AddStatement(Simple);

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

        void AddStatement(StatementType type)
        {
            statements.Add(new Statement
            {
                Args = args.ToArray(),
                Type = type
            });
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
