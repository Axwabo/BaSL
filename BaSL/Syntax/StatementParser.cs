using System.Collections.Generic;

namespace BaSL.Syntax;

internal static class StatementParser
{

    public static List<Statement> Parse(string line)
    {
        // TODO: escaping & shit
        var statements = line.Split(';');
    }

}
