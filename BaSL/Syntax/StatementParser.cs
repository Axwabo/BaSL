using System;
using System.Collections.Generic;
using System.Linq;

namespace BaSL.Syntax;

internal static class StatementParser
{

    // TODO: escaping & shit
    public static List<Statement> Parse(string line) => line.Split(';').Select(ParseStatement).ToList();

    private static Statement ParseStatement(string text)
    {
        throw new NotImplementedException();
    }

}
