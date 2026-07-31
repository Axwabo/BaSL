namespace BaSL.Syntax;

internal sealed class Statement
{

    public required string[] Args { get; init; }

    public required StatementType Type { get; init; }

}
