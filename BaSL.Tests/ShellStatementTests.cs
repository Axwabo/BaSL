using BaSL.Syntax;

namespace BaSL.Tests;

public sealed class ShellStatementTests
{

    [Fact]
    public void Pipe()
    {
        var source = new Args("echo");
        var target = new Args("cat");
        var statement = source | target;
        Assert.Equal(statement, new PipeStatement(new StandaloneStatement("echo"), "cat"));
    }

}
