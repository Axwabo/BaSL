using System;

namespace BaSL.Syntax;

internal sealed class Statement
{

    public required ReadOnlyMemory<string> Args { get; init; }

    public required StatementType Type { get; init; }

}
