using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BaSL.Syntax;

internal delegate bool TryParse(string variable, [NotNullWhen(true)] out string? result);

internal static class StatementParser
{

    // TODO: escaping & shit
    public static List<ShellStatement> Parse(string line, TryParse variables)
    {
        var statements = new List<ShellStatement>();
        foreach (var s in line.Split(';'))
            if (ParseStatement(s, variables) is { } statement)
                statements.Add(statement);
        return statements;
    }

    private static ShellStatement? ParseStatement(string s, TryParse variables)
    {
        ShellStatement? statement = null;
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
                    AddStatement(StatementType.Pipe);
                    break;
                case (SyntaxType.Text, '>') when next == '>':
                    AddStatement(StatementType.RedirectStandardOutputAppend);
                    i++;
                    break;
                case (SyntaxType.Text, '>'):
                    AddStatement(StatementType.RedirectStandardOutputOverwrite);
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
            AddStatement(StatementType.Simple);

        return statement;

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
            statement = (type, statement) switch
            {
                (StatementType.Simple, _) => StandaloneStatement.FromArgs(args),
                // (StatementType.Pipe, not null) => statement | StandaloneStatement.FromArgs(args),
                (StatementType.RedirectStandardOutputAppend, not null) => statement >> ,
                _ => null
            };
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
